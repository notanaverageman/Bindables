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
	public class DependencyPropertyWeaver : PropertyWeaverBase
	{
		private readonly TypeReference _dependencyPropertyKey;

		private readonly MethodReference _registerDependencyProperty;
		private readonly MethodReference _registerDependencyPropertyReadOnly;

		private readonly MethodReference _setValueDependencyPropertyKey;
		private readonly MethodReference _getDependencyProperty;

		public DependencyPropertyWeaver(ModuleDefinition moduleDefinition) : base(moduleDefinition)
		{
			_dependencyPropertyKey = moduleDefinition.ImportReference(typeof(DependencyPropertyKey));

			_registerDependencyProperty = moduleDefinition.ImportMethod(typeof(DependencyProperty), nameof(DependencyProperty.Register), typeof(string), typeof(Type), typeof(Type), typeof(PropertyMetadata));
			_registerDependencyPropertyReadOnly = moduleDefinition.ImportMethod(typeof(DependencyProperty), nameof(DependencyProperty.RegisterReadOnly), typeof(string), typeof(Type), typeof(Type), typeof(PropertyMetadata));

			_setValueDependencyPropertyKey = moduleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.SetValue), typeof(DependencyPropertyKey), typeof(object));
			_getDependencyProperty = moduleDefinition.ImportMethod(typeof(DependencyPropertyKey), $"get_{nameof(DependencyPropertyKey.DependencyProperty)}");
		}

		protected override void ExecuteInternal(TypeDefinition type, List<PropertyDefinition> propertiesToConvert)
		{
			Dictionary<PropertyDefinition, Range> instructionRanges = type.InterpretDefaultValues(propertiesToConvert);

			foreach (PropertyDefinition property in propertiesToConvert)
			{
				property.ValidateBeforeDependencyPropertyConversion(type);

				Range instructionRange = instructionRanges[property];
				ConvertToDependencyProperty(type, property, instructionRange);

				property.RemoveBindableAttributes();
			}

			foreach (MethodDefinition constructor in type.GetConstructors().Where(constructor => constructor != type.GetStaticConstructor()))
			{
				constructor.RemoveInitializationInstructionsFromConstructor(instructionRanges);
			}
		}

		protected override void AppendDefaultValueInstructions(List<Instruction> instructions, TypeDefinition type, PropertyDefinition property, Range instructionRange)
		{
			instructions.AppendDefaultValueInstructionsForDependencyProperty(type, property, instructionRange);
		}

		protected override bool ShouldConvert(TypeDefinition type, PropertyDefinition property)
		{
			CustomAttribute typeAttribute = type.GetDependencyPropertyAttribute();
			CustomAttribute propertyAttribute = property.GetDependencyPropertyAttribute();

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

				if (property.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(ExcludeDependencyPropertyAttribute).FullName))
				{
					return false;
				}
			}

			return true;
		}

		private void ConvertToDependencyProperty(TypeDefinition type, PropertyDefinition property, Range instructionRange)
		{
			string fieldName = property.Name + "Property";
			FieldDefinition field = new FieldDefinition(fieldName, FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Public, DependencyPropertyType);

			type.Fields.Add(field);

			if (property.IsMarkedAsReadOnly())
			{
				string dependencyPropertyKeyFieldName = property.Name + "PropertyKey";
				FieldDefinition dependencyPropertyKeyField = new FieldDefinition(dependencyPropertyKeyFieldName, FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Private, _dependencyPropertyKey);

				type.Fields.Add(dependencyPropertyKeyField);

				AddInitializationToStaticConstructorReadonly(type, property, dependencyPropertyKeyField, field, instructionRange);
				ModifySetMethod(property, dependencyPropertyKeyField, true);
			}
			else
			{
				AddInitializationToStaticConstructor(type, property, field, instructionRange);
				ModifySetMethod(property, field, false);
			}

			ModifyGetMethod(property, field);

			FieldDefinition backingField = type.GetBackingFieldForProperty(property);
			type.Fields.Remove(backingField);
		}

		private void AddInitializationToStaticConstructor(TypeDefinition type, PropertyDefinition property, FieldDefinition field, Range instructionRange)
		{
			MethodDefinition staticConstructor = type.GetStaticConstructor();

			List<Instruction> instructions = CreateInstructionsUpToRegistration(type, property, instructionRange, property.GetDependencyPropertyAttribute());

			instructions.Add(Instruction.Create(OpCodes.Call, _registerDependencyProperty));
			instructions.Add(Instruction.Create(OpCodes.Stsfld, field));

			instructions.Reverse();

			foreach (Instruction instruction in instructions)
			{
				staticConstructor.Body.Instructions.Insert(0, instruction);
			}
		}

		private void AddInitializationToStaticConstructorReadonly(TypeDefinition type, PropertyDefinition property, FieldDefinition dependencyPropertyKeyField, FieldDefinition field, Range instructionRange)
		{
			MethodDefinition staticConstructor = type.GetStaticConstructor();

			List<Instruction> instructions = CreateInstructionsUpToRegistration(type, property, instructionRange, property.GetDependencyPropertyAttribute());

			instructions.Add(Instruction.Create(OpCodes.Call, _registerDependencyPropertyReadOnly));
			instructions.Add(Instruction.Create(OpCodes.Stsfld, dependencyPropertyKeyField));

			instructions.Add(Instruction.Create(OpCodes.Ldsfld, dependencyPropertyKeyField));
			instructions.Add(Instruction.Create(OpCodes.Callvirt, _getDependencyProperty));
			instructions.Add(Instruction.Create(OpCodes.Stsfld, field));

			instructions.Reverse();

			foreach (Instruction instruction in instructions)
			{
				staticConstructor.Body.Instructions.Insert(0, instruction);
			}
		}

		private void ModifyGetMethod(PropertyDefinition property, FieldDefinition field)
		{
			MethodDefinition getter = property.GetMethod;
			Collection<Instruction> getterInstructions = getter.Body.Instructions;

			getterInstructions.Clear();
			getterInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			getterInstructions.Add(Instruction.Create(OpCodes.Ldsfld, field));
			getterInstructions.Add(Instruction.Create(OpCodes.Call, GetValue));
			getterInstructions.Add(property.PropertyType.IsValueType
				? Instruction.Create(OpCodes.Unbox_Any, property.PropertyType)
				: Instruction.Create(OpCodes.Castclass, property.PropertyType));
			getterInstructions.Add(Instruction.Create(OpCodes.Ret));
		}

		private void ModifySetMethod(PropertyDefinition property, FieldDefinition field, bool isReadonly)
		{
			MethodDefinition setter = property.SetMethod;
			Collection<Instruction> setterInstructions = setter.Body.Instructions;

			setterInstructions.Clear();
			setterInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			setterInstructions.Add(Instruction.Create(OpCodes.Ldsfld, field));
			setterInstructions.Add(Instruction.Create(OpCodes.Ldarg_1));
			if (property.PropertyType.IsValueType)
			{
				setterInstructions.Add(Instruction.Create(OpCodes.Box, property.PropertyType));
			}
			setterInstructions.Add(isReadonly
				? Instruction.Create(OpCodes.Call, _setValueDependencyPropertyKey)
				: Instruction.Create(OpCodes.Call, SetValue));
			setterInstructions.Add(Instruction.Create(OpCodes.Ret));
		}
	}
}