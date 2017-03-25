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
		private readonly TypeReference _dependencyObjectType;
		private readonly MethodReference _registerAttachedProperty;

		public AttachedPropertyWeaver(ModuleDefinition moduleDefinition) : base(moduleDefinition)
		{
			_dependencyObjectType = moduleDefinition.ImportReference(typeof(DependencyObject));
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

			ModifyGetMethod(type, property, field);
			ModifySetMethod(type, property, field);

			FieldDefinition backingField = type.GetBackingFieldForProperty(property);
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

		private void ModifyGetMethod(TypeDefinition type, PropertyDefinition property, FieldDefinition field)
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

		private void ModifySetMethod(TypeDefinition type, PropertyDefinition property, FieldDefinition field)
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
			// There are two candidate methods:
			//   1. The getter of the property.
			//   2. static T GetProperty(DependencyObject, T value) method.
			//  
			// If we have the second method and it has body other than 'throw new WillBeImplementedByBindablesException()'
			// we will throw an exception. This is to prevent any unexpected behavior since we will overwrite this method's
			// contents. If the second method exists we will delete the first method as it will cause ambiguity.
			// 
			// Otherwise we will change the signature of the getter method to create the second method as the getter will not
			// (should not) be used in the program.

			MethodDefinition getter = property.GetMethod;
			MethodDefinition getPropertyMethod = null;

			bool useGetter = false;

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
				useGetter = true;
			}

			if (useGetter)
			{
				getter.Name = getter.Name.Replace("get_", "Get");
				getter.IsHideBySig = false;
				getter.IsSpecialName = false;

				ParameterDefinition parameterDefinition = new ParameterDefinition(_dependencyObjectType);
				getter.Parameters.Add(parameterDefinition);

				return getter;
			}

			type.Methods.Remove(getter);
			getPropertyMethod.Body.Instructions.Clear();

			return getPropertyMethod;
		}

		private MethodDefinition AcquireSetMethod(TypeDefinition type, PropertyDefinition property)
		{
			// See the comments on AcquireGetMethod method.

			MethodDefinition setter = property.SetMethod;
			MethodDefinition setPropertyMethod = null;

			bool useSetter = false;

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
				useSetter = true;
			}

			if (useSetter)
			{
				setter.Name = setter.Name.Replace("set_", "Set");
				setter.IsHideBySig = false;
				setter.IsSpecialName = false;

				ParameterDefinition parameterDefinition = new ParameterDefinition(_dependencyObjectType);
				setter.Parameters.Insert(0, parameterDefinition);

				return setter;
			}

			type.Methods.Remove(setter);
			return setPropertyMethod;
		}
	}
}