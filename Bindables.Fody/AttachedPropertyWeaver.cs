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

			ModifyGetMethod(property, field);
			ModifySetMethod(property, field);

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

		private void ModifyGetMethod(PropertyDefinition property, FieldDefinition field)
		{
			MethodDefinition getter = property.GetMethod;

			getter.Name = getter.Name.Replace("get_", "Get");
			getter.IsHideBySig = false;
			getter.IsSpecialName = false;

			ParameterDefinition parameterDefinition = new ParameterDefinition(_dependencyObjectType);
			getter.Parameters.Add(parameterDefinition);

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

		private void ModifySetMethod(PropertyDefinition property, FieldDefinition field)
		{
			MethodDefinition setter = property.SetMethod;

			setter.Name = setter.Name.Replace("set_", "Set");
			setter.IsHideBySig = false;
			setter.IsSpecialName = false;

			ParameterDefinition parameterDefinition = new ParameterDefinition(_dependencyObjectType);
			setter.Parameters.Insert(0, parameterDefinition);

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
	}
}