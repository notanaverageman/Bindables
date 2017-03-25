using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Bindables.Fody
{
	public class AttachedPropertyWeaver : PropertyWeaverBase
	{
		private readonly MethodReference _registerAttachedProperty;

		public AttachedPropertyWeaver(ModuleDefinition moduleDefinition) : base(moduleDefinition)
		{
			_registerAttachedProperty = moduleDefinition.ImportMethod(typeof(DependencyProperty), nameof(DependencyProperty.RegisterAttached), typeof(string), typeof(Type), typeof(Type), typeof(PropertyMetadata));
		}

		protected override void ExecuteInternal(TypeDefinition type, List<PropertyDefinition> propertiesToConvert)
		{
			Dictionary<PropertyDefinition, Range> instructionRanges = type.InterpretDefaultValuesStatic(propertiesToConvert);

			int startIndex = 0;

			if (instructionRanges.Any(r => r.Value != null))
			{
				startIndex = instructionRanges.Where(r => r.Value != null).Max(r => r.Value.End) + 1;
			}

			foreach (PropertyDefinition property in propertiesToConvert)
			{
				property.ValidateBeforeAttachedPropertyConversion(type);

				Range instructionRange = instructionRanges[property];
				ConvertToAttachedProperty(type, property, instructionRange, startIndex);

				property.RemoveBindableAttributes();
			}

			type.GetStaticConstructor().RemoveInitializationInstructionsFromConstructor(instructionRanges);
		}

		protected override void AppendDefaultValueInstructions(List<Instruction> instructions, TypeDefinition type, PropertyDefinition property, Range instructionRange)
		{
			instructions.AppendDefaultValueInstructionsForAttachedProperty(type, property, instructionRange);
		}

		protected override bool ShouldConvert(TypeDefinition type, PropertyDefinition property)
		{
			CustomAttribute typeAttribute = type.GetAttachedPropertyAttribute();
			CustomAttribute propertyAttribute = property.GetAttachedPropertyAttribute();

			FieldDefinition backingField = type.GetBackingFieldForProperty(property);

			if (typeAttribute == null && propertyAttribute == null)
			{
				return false;
			}

			if (typeAttribute != null)
			{
				if (backingField == null)
				{
					return false;
				}

				if (property.SetMethod == null)
				{
					return false;
				}

				if (!property.GetMethod.IsStatic)
				{
					return false;
				}

				if (property.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(ExcludeAttachedPropertyAttribute).FullName))
				{
					return false;
				}
			}

			return true;
		}

		private void ConvertToAttachedProperty(TypeDefinition type, PropertyDefinition property, Range instructionRange, int startIndex)
		{
			string fieldName = property.Name + "Property";
			FieldDefinition field = new FieldDefinition(fieldName, FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Public, DependencyPropertyType);

			type.Fields.Add(field);

			AddInitializationToStaticConstructor(type, property, field, instructionRange, startIndex);

			CreateGetMethod(type, property, field);
			CreateSetMethod(type, property, field);

			FieldDefinition backingField = type.GetBackingFieldForProperty(property);
			
			type.Methods.Remove(property.GetMethod);
			type.Methods.Remove(property.SetMethod);
			type.Properties.Remove(property);
			type.Fields.Remove(backingField);
		}

		private void AddInitializationToStaticConstructor(TypeDefinition type, PropertyDefinition property, FieldDefinition field, Range instructionRange, int startIndex)
		{
			MethodDefinition staticConstructor = type.GetStaticConstructor();

			List<Instruction> instructions = CreateInstructionsUpToRegistration(type, property, instructionRange, property.GetAttachedPropertyAttribute());

			instructions.Add(Instruction.Create(OpCodes.Call, _registerAttachedProperty));
			instructions.Add(Instruction.Create(OpCodes.Stsfld, field));

			instructions.Reverse();

			foreach (Instruction instruction in instructions)
			{
				staticConstructor.Body.Instructions.Insert(startIndex, instruction);
			}
		}

		private void CreateGetMethod(TypeDefinition type, PropertyDefinition property, FieldDefinition field)
		{
			MethodDefinition getter = AcquireGetMethod(type, property);

			Collection<Instruction> getterInstructions = getter.Body.Instructions;

			getterInstructions.Clear();
			getterInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			getterInstructions.Add(Instruction.Create(OpCodes.Ldsfld, field));
			getterInstructions.Add(Instruction.Create(OpCodes.Callvirt, GetValue));
			getterInstructions.Add(property.PropertyType.IsValueType
				? Instruction.Create(OpCodes.Unbox_Any, property.PropertyType)
				: Instruction.Create(OpCodes.Castclass, property.PropertyType));
			getterInstructions.Add(Instruction.Create(OpCodes.Ret));
		}

		private void CreateSetMethod(TypeDefinition type, PropertyDefinition property, FieldDefinition field)
		{
			MethodDefinition setter = AcquireSetMethod(type, property);

			Collection<Instruction> setterInstructions = setter.Body.Instructions;

			setterInstructions.Clear();
			setterInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			setterInstructions.Add(Instruction.Create(OpCodes.Ldsfld, field));
			setterInstructions.Add(Instruction.Create(OpCodes.Ldarg_1));
			if (property.PropertyType.IsValueType)
			{
				setterInstructions.Add(Instruction.Create(OpCodes.Box, property.PropertyType));
			}
			setterInstructions.Add(Instruction.Create(OpCodes.Callvirt, SetValue));
			setterInstructions.Add(Instruction.Create(OpCodes.Ret));
		}

		private MethodDefinition AcquireGetMethod(TypeDefinition type, PropertyDefinition property)
		{
			// If we have the static GetProperty(DependencyObject) method and it has body other than 'throw new WillBeImplementedByBindablesException()'
			// we will throw an exception. This is to prevent any unexpected behavior since we will overwrite this method's
			// contents.
			// We will delete the property's get method here.

			MethodDefinition getPropertyMethod;

			try
			{
				getPropertyMethod = ModuleDefinition.ImportMethod(type, "Get" + property.Name, DependencyObjectType).Resolve();

				if (!getPropertyMethod.IsStatic)
				{
					throw new WeavingException($"There is a method with signature {getPropertyMethod.FullName}, but it is not static.");
				}

				string message = $"The method: {getPropertyMethod.FullName} should have only one instruction and it should be: 'throw new WillBeImplementedByBindablesException();'.";

				List<Instruction> bodyInstructions = getPropertyMethod.Body.Instructions.Where(x => x.OpCode != OpCodes.Nop).ToList();

				if (bodyInstructions.Count != 2)
				{
					throw new WeavingException(message);
				}

				MethodReference exceptionConstructor = bodyInstructions[0].Operand as MethodReference;
				TypeReference exceptionType = exceptionConstructor?.DeclaringType;

				bool firstInstructionIsNewobj = bodyInstructions[0].OpCode == OpCodes.Newobj;
				bool firstInstructionOperandIsWillBeImplementedByBindablesException = exceptionType?.Name == nameof(WillBeImplementedByBindablesException);
				bool secondInstructionIsThrow = bodyInstructions[1].OpCode == OpCodes.Throw;

				if (!firstInstructionIsNewobj || !firstInstructionOperandIsWillBeImplementedByBindablesException || !secondInstructionIsThrow)
				{
					throw new WeavingException(message);
				}

				if (getPropertyMethod.ReturnType != property.PropertyType)
				{
					throw new WeavingException($"The method: {getPropertyMethod.FullName} should return {property.PropertyType}.");
				}
			}
			catch (ArgumentException)
			{
				getPropertyMethod = new MethodDefinition("Get" + property.Name, MethodAttributes.Public | MethodAttributes.Static, property.PropertyType)
				{
					IsHideBySig = false,
					IsSpecialName = false
				};
				getPropertyMethod.Parameters.Add(new ParameterDefinition(DependencyObjectType));

				type.Methods.Add(getPropertyMethod);
			}

			getPropertyMethod.Body.Instructions.Clear();

			return getPropertyMethod;
		}

		private MethodDefinition AcquireSetMethod(TypeDefinition type, PropertyDefinition property)
		{
			// See the comments on AcquireGetMethod method.

			MethodDefinition setter = property.SetMethod;
			MethodDefinition setPropertyMethod;

			try
			{
				setPropertyMethod = ModuleDefinition.ImportMethod(type, "Set" + property.Name, DependencyObjectType, property.PropertyType).Resolve();

				if (!setPropertyMethod.IsStatic)
				{
					throw new WeavingException($"There is a method with signature {setPropertyMethod.FullName}, but it is not static.");
				}

				if (setPropertyMethod.Body.Instructions.Any(x => x.OpCode != OpCodes.Nop && x.OpCode != OpCodes.Ret))
				{
					throw new WeavingException($"The method: {setPropertyMethod.FullName} should not have any instructions.");
				}

				if (setPropertyMethod.ReturnType != ModuleDefinition.TypeSystem.Void)
				{
					throw new WeavingException($"The method: {setPropertyMethod.FullName} should return void.");
				}
			}
			catch (ArgumentException)
			{
				setPropertyMethod = new MethodDefinition("Set" + property.Name, MethodAttributes.Public | MethodAttributes.Static, ModuleDefinition.TypeSystem.Void)
				{
					IsHideBySig = false,
					IsSpecialName = false
				};
				setPropertyMethod.Parameters.Add(new ParameterDefinition(DependencyObjectType));
				setPropertyMethod.Parameters.Add(new ParameterDefinition(property.PropertyType));

				type.Methods.Add(setPropertyMethod);
			}

			type.Methods.Remove(setter);
			return setPropertyMethod;
		}
	}
}