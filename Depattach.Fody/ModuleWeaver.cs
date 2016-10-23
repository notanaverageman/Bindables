using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Depattach.Fody
{
	public class ModuleWeaver
	{
		private const string DependencyPropertyLibraryName = "DepAttachMarker";

		private TypeReference _dependencyObjectTypeReference;
		private MethodReference _getTypeFromHandleMethodReference;
		private MethodReference _registerDependencyProperty;
		private MethodReference _setValue;
		private MethodReference _getValue;
		private MethodReference _frameworkPropertyMetadataConstructor;

		public Action<string> LogInfo { get; set; }
		public ModuleDefinition ModuleDefinition { get; set; }

		public ModuleWeaver()
		{
			LogInfo = m => { };
		}

		public void Execute()
		{
			AssemblyNameReference dependencyPropertyAssemblyNameReference = ModuleDefinition.AssemblyReferences.FirstOrDefault(reference => reference.Name == DependencyPropertyLibraryName);

			_dependencyObjectTypeReference = ModuleDefinition.ImportReference(typeof(DependencyObject));
			_getTypeFromHandleMethodReference = ModuleDefinition.ImportMethod(typeof(Type), nameof(Type.GetTypeFromHandle), typeof(RuntimeTypeHandle));
			_registerDependencyProperty = ModuleDefinition.ImportMethod(typeof(DependencyProperty), nameof(DependencyProperty.Register), typeof(string), typeof(Type), typeof(Type), typeof(PropertyMetadata));
			_setValue = ModuleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.SetValue), typeof(DependencyProperty), typeof(object));
			_getValue = ModuleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.GetValue), typeof(DependencyProperty));
			_frameworkPropertyMetadataConstructor = ModuleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object));

			foreach (TypeDefinition typeDefinition in ModuleDefinition.Types)
			{
				foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
				{
					if (!propertyDefinition.CanConvertToDependencyProperty(typeDefinition))
					{
						continue;
					}

					propertyDefinition.ValidateBeforeConversion(typeDefinition);
					typeDefinition.ValidateBeforeConversion(_dependencyObjectTypeReference);

					typeDefinition.CreateStaticConstructorIfNotExists();

					ConvertToDependencyProperty(typeDefinition, propertyDefinition);

					propertyDefinition.RemoveDependencyPropertyAttribute();
				}

				typeDefinition.RemoveDependencyPropertyAttribute();
			}

			ModuleDefinition.AssemblyReferences.Remove(dependencyPropertyAssemblyNameReference);
		}

		private void ConvertToDependencyProperty(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
		{
			FieldDefinition backingField = typeDefinition.GetBackingFieldForProperty(propertyDefinition);

			string fieldName = propertyDefinition.Name + "Property";
			FieldDefinition fieldDefinition = new FieldDefinition(fieldName, FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Public, _dependencyObjectTypeReference);

			object defaultValue = null;// propertyDefinition.GetDependencyPropertyAttribute()?.GetDefaultValueFromAttribute();

			AddInitializationToStaticConstructor(typeDefinition, propertyDefinition, fieldDefinition, defaultValue);

			typeDefinition.Fields.Add(fieldDefinition);

			CreateGetMethod(propertyDefinition, fieldDefinition);
			CreateSetMethod(propertyDefinition, fieldDefinition);

			typeDefinition.Fields.Remove(backingField);
		}

		private void AddInitializationToStaticConstructor(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition, FieldDefinition fieldDefinition, object defaultValue)
		{
			MethodDefinition staticConstructor = typeDefinition.GetStaticConstructor();

			List<Instruction> instructions = new List<Instruction>
			{
				Instruction.Create(OpCodes.Ldstr, propertyDefinition.Name),
				Instruction.Create(OpCodes.Ldtoken, propertyDefinition.PropertyType),
				Instruction.Create(OpCodes.Call, _getTypeFromHandleMethodReference),
				Instruction.Create(OpCodes.Ldtoken, typeDefinition),
				Instruction.Create(OpCodes.Call, _getTypeFromHandleMethodReference)
			};

			//if (defaultValue != null)
			//{
			//	Type type = defaultValue.GetType();
			//	if (type == typeof(string))
			//	{
			//		instructions.AddInstrunctionForDefaultValue((string)defaultValue);
			//	}
			//	else if (type == typeof(int))
			//	{
			//		instructions.AddInstrunctionForDefaultValue((int)defaultValue, ModuleDefinition);
			//	}
			//	else if (type == typeof(bool))
			//	{
			//		instructions.AddInstrunctionForDefaultValue((bool)defaultValue, ModuleDefinition);
			//	}
			//}
			//else
			//{
			VariableDefinition variable = instructions.AddInstrunctionForDefaultValue(propertyDefinition);

			if (variable != null)
			{
				staticConstructor.Body.InitLocals = true;
				staticConstructor.Body.Variables.Add(variable);
			}
			//}

			instructions.Add(Instruction.Create(OpCodes.Newobj, _frameworkPropertyMetadataConstructor));

			instructions.Add(Instruction.Create(OpCodes.Call, _registerDependencyProperty));
			instructions.Add(Instruction.Create(OpCodes.Stsfld, fieldDefinition));

			instructions.Reverse();

			foreach (Instruction instruction in instructions)
			{
				staticConstructor.Body.Instructions.Insert(0, instruction);
			}
		}

		private void CreateSetMethod(PropertyDefinition propertyDefinition, FieldDefinition fieldDefinition)
		{
			MethodDefinition setter = propertyDefinition.SetMethod;
			Collection<Instruction> setterInstructions = setter.Body.Instructions;

			setterInstructions.Clear();
			setterInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			setterInstructions.Add(Instruction.Create(OpCodes.Ldsfld, fieldDefinition));
			setterInstructions.Add(Instruction.Create(OpCodes.Ldarg_1));
			if (propertyDefinition.PropertyType.IsValueType)
			{
				setterInstructions.Add(Instruction.Create(OpCodes.Box, propertyDefinition.PropertyType));
			}
			setterInstructions.Add(Instruction.Create(OpCodes.Call, _setValue));
			setterInstructions.Add(Instruction.Create(OpCodes.Ret));
		}

		private void CreateGetMethod(PropertyDefinition propertyDefinition, FieldDefinition fieldDefinition)
		{
			MethodDefinition getter = propertyDefinition.GetMethod;
			Collection<Instruction> getterInstructions = getter.Body.Instructions;

			getterInstructions.Clear();
			getterInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			getterInstructions.Add(Instruction.Create(OpCodes.Ldsfld, fieldDefinition));
			getterInstructions.Add(Instruction.Create(OpCodes.Call, _getValue));
			getterInstructions.Add(propertyDefinition.PropertyType.IsValueType
				? Instruction.Create(OpCodes.Unbox_Any, propertyDefinition.PropertyType)
				: Instruction.Create(OpCodes.Castclass, propertyDefinition.PropertyType));
			getterInstructions.Add(Instruction.Create(OpCodes.Ret));
		}
	}
}