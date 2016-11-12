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
	public class ModuleWeaver
	{
		private const string DependencyPropertyLibraryName = "Bindables";

		private TypeReference _dependencyPropertyKeyTypeReference;
		private TypeReference _dependencyObjectTypeReference;

		private MethodReference _getTypeFromHandleMethodReference;

		private MethodReference _registerDependencyProperty;
		private MethodReference _registerDependencyPropertyReadOnly;

		private MethodReference _setValue;
		private MethodReference _setValueDependencyPropertyKey;
		private MethodReference _getValue;
		private MethodReference _getDependencyProperty;

		private MethodReference _propertyChangedCallbackConstructor;

		private MethodReference _frameworkPropertyMetadataConstructor;
		private MethodReference _frameworkPropertyMetadataConstructorWithOptions;
		private MethodReference _frameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback;

		public Action<string> LogInfo { get; set; }
		public ModuleDefinition ModuleDefinition { get; set; }

		public ModuleWeaver()
		{
			LogInfo = m => { };
		}

		public void Execute()
		{
			AssemblyNameReference dependencyPropertyAssemblyNameReference = ModuleDefinition.AssemblyReferences.FirstOrDefault(reference => reference.Name == DependencyPropertyLibraryName);

			_dependencyPropertyKeyTypeReference = ModuleDefinition.ImportReference(typeof(DependencyPropertyKey));
			_dependencyObjectTypeReference = ModuleDefinition.ImportReference(typeof(DependencyObject));

			_getTypeFromHandleMethodReference = ModuleDefinition.ImportMethod(typeof(Type), nameof(Type.GetTypeFromHandle), typeof(RuntimeTypeHandle));

			_registerDependencyProperty = ModuleDefinition.ImportMethod(typeof(DependencyProperty), nameof(DependencyProperty.Register), typeof(string), typeof(Type), typeof(Type), typeof(PropertyMetadata));
			_registerDependencyPropertyReadOnly = ModuleDefinition.ImportMethod(typeof(DependencyProperty), nameof(DependencyProperty.RegisterReadOnly), typeof(string), typeof(Type), typeof(Type), typeof(PropertyMetadata));

			_setValue = ModuleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.SetValue), typeof(DependencyProperty), typeof(object));
			_setValueDependencyPropertyKey = ModuleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.SetValue), typeof(DependencyPropertyKey), typeof(object));
			_getValue = ModuleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.GetValue), typeof(DependencyProperty));
			_getDependencyProperty = ModuleDefinition.ImportMethod(typeof(DependencyPropertyKey), $"get_{nameof(DependencyPropertyKey.DependencyProperty)}");

			_propertyChangedCallbackConstructor = ModuleDefinition.ImportSingleConstructor(typeof(PropertyChangedCallback));

			_frameworkPropertyMetadataConstructor = ModuleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object));
			_frameworkPropertyMetadataConstructorWithOptions = ModuleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object), typeof(FrameworkPropertyMetadataOptions));
			_frameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback = ModuleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object), typeof(FrameworkPropertyMetadataOptions), typeof(PropertyChangedCallback));

			foreach (TypeDefinition typeDefinition in ModuleDefinition.Types)
			{
				List<PropertyDefinition> propertiesToConvert = new List<PropertyDefinition>();

				foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
				{
					if (!propertyDefinition.ShouldConvertToDependencyProperty(typeDefinition))
					{
						continue;
					}

					propertiesToConvert.Add(propertyDefinition);
				}

				if (!propertiesToConvert.Any())
				{
					continue;
				}

				typeDefinition.CreateStaticConstructorIfNotExists();

				Dictionary<PropertyDefinition, Range> instructionRanges = typeDefinition.InterpretDefaultValues(propertiesToConvert);

				foreach (PropertyDefinition propertyDefinition in propertiesToConvert)
				{
					propertyDefinition.ValidateBeforeConversion(typeDefinition);
					typeDefinition.ValidateBeforeConversion(_dependencyObjectTypeReference);

					Range range = instructionRanges[propertyDefinition];
					ConvertToDependencyProperty(typeDefinition, propertyDefinition, range);

					propertyDefinition.RemoveDependencyPropertyAttribute();
				}

				typeDefinition.RemoveInitializationInstructionsFromAllConstructors(instructionRanges);
				typeDefinition.RemoveDependencyPropertyAttribute();
			}

			ModuleDefinition.AssemblyReferences.Remove(dependencyPropertyAssemblyNameReference);
		}

		private void ConvertToDependencyProperty(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition, Range range)
		{
			FieldDefinition backingField = typeDefinition.GetBackingFieldForProperty(propertyDefinition);

			string fieldName = propertyDefinition.Name + "Property";
			FieldDefinition fieldDefinition = new FieldDefinition(fieldName, FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Public, _dependencyObjectTypeReference);

			typeDefinition.Fields.Add(fieldDefinition);

			if (propertyDefinition.IsReadOnly())
			{
				string dependencyPropertyKeyFieldName = propertyDefinition.Name + "PropertyKey";
				FieldDefinition dependencyPropertyKeyFieldDefinition = new FieldDefinition(dependencyPropertyKeyFieldName, FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Private, _dependencyPropertyKeyTypeReference);

				typeDefinition.Fields.Add(dependencyPropertyKeyFieldDefinition);

				AddInitializationToStaticConstructorReadonly(typeDefinition, propertyDefinition, dependencyPropertyKeyFieldDefinition, fieldDefinition, range);

				propertyDefinition.SetMethod.Body.Instructions.Clear();
				propertyDefinition.CustomAttributes.Clear();

				ModifySetMethod(propertyDefinition, dependencyPropertyKeyFieldDefinition, true);
			}
			else
			{
				AddInitializationToStaticConstructor(typeDefinition, propertyDefinition, fieldDefinition, range);
				ModifySetMethod(propertyDefinition, fieldDefinition, false);
			}

			ModifyGetMethod(propertyDefinition, fieldDefinition);

			typeDefinition.Fields.Remove(backingField);
		}

		private void AddInitializationToStaticConstructor(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition, FieldDefinition fieldDefinition, Range range)
		{
			MethodDefinition staticConstructor = typeDefinition.GetStaticConstructor();

			List<Instruction> instructions = CreateInstructionsUpToRegistration(typeDefinition, propertyDefinition, range);

			instructions.Add(Instruction.Create(OpCodes.Call, _registerDependencyProperty));
			instructions.Add(Instruction.Create(OpCodes.Stsfld, fieldDefinition));

			instructions.Reverse();

			foreach (Instruction instruction in instructions)
			{
				staticConstructor.Body.Instructions.Insert(0, instruction);
			}
		}

		private void AddInitializationToStaticConstructorReadonly(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition, FieldDefinition dependencyPropertyKeyFieldDefinition, FieldDefinition fieldDefinition, Range range)
		{
			MethodDefinition staticConstructor = typeDefinition.GetStaticConstructor();

			List<Instruction> instructions = CreateInstructionsUpToRegistration(typeDefinition, propertyDefinition, range);

			instructions.Add(Instruction.Create(OpCodes.Call, _registerDependencyPropertyReadOnly));
			instructions.Add(Instruction.Create(OpCodes.Stsfld, dependencyPropertyKeyFieldDefinition));

			instructions.Add(Instruction.Create(OpCodes.Ldsfld, dependencyPropertyKeyFieldDefinition));
			instructions.Add(Instruction.Create(OpCodes.Callvirt, _getDependencyProperty));
			instructions.Add(Instruction.Create(OpCodes.Stsfld, fieldDefinition));

			instructions.Reverse();

			foreach (Instruction instruction in instructions)
			{
				staticConstructor.Body.Instructions.Insert(0, instruction);
			}
		}

		private List<Instruction> CreateInstructionsUpToRegistration(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition, Range range)
		{
			List<Instruction> instructions = new List<Instruction>
			{
				Instruction.Create(OpCodes.Ldstr, propertyDefinition.Name),
				Instruction.Create(OpCodes.Ldtoken, propertyDefinition.PropertyType),
				Instruction.Create(OpCodes.Call, _getTypeFromHandleMethodReference),
				Instruction.Create(OpCodes.Ldtoken, typeDefinition),
				Instruction.Create(OpCodes.Call, _getTypeFromHandleMethodReference)
			};

			instructions.AppendDefaultValueInstructions(typeDefinition, propertyDefinition, range);

			CustomAttribute dependencyPropertyAttribute = propertyDefinition.GetDependencyPropertyAttribute();
			if (dependencyPropertyAttribute?.HasProperties == true)
			{
				CustomAttributeArgument options = dependencyPropertyAttribute.Properties.FirstOrDefault(p => p.Name == nameof(DependencyPropertyAttribute.Options)).Argument;

				instructions.Add(options.Value == null
					? Instruction.Create(OpCodes.Ldc_I4, (int) FrameworkPropertyMetadataOptions.None)
					: Instruction.Create(OpCodes.Ldc_I4, (int) options.Value));

				string propertyChangedCallback = dependencyPropertyAttribute.Properties.FirstOrDefault(p => p.Name == nameof(DependencyPropertyAttribute.OnPropertyChanged)).Argument.Value as string;
				if (String.IsNullOrEmpty(propertyChangedCallback))
				{
					instructions.Add(Instruction.Create(OpCodes.Newobj, _frameworkPropertyMetadataConstructorWithOptions));
				}
				else
				{
					try
					{
						MethodReference method = ModuleDefinition.ImportMethod(typeDefinition, propertyChangedCallback, typeof(DependencyObject), typeof(DependencyPropertyChangedEventArgs));

						instructions.Add(Instruction.Create(OpCodes.Ldnull));
						instructions.Add(Instruction.Create(OpCodes.Ldftn, method));
						instructions.Add(Instruction.Create(OpCodes.Newobj, _propertyChangedCallbackConstructor));
						instructions.Add(Instruction.Create(OpCodes.Newobj, _frameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback));
					}
					catch (ArgumentException)
					{
						throw new InvalidOperationException();
					}
				}
			}
			else
			{
				instructions.Add(Instruction.Create(OpCodes.Newobj, _frameworkPropertyMetadataConstructor));
			}

			return instructions;
		}

		private void ModifyGetMethod(PropertyDefinition propertyDefinition, FieldDefinition fieldDefinition)
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

		private void ModifySetMethod(PropertyDefinition propertyDefinition, FieldDefinition fieldDefinition, bool isReadonly)
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
			setterInstructions.Add(isReadonly
				? Instruction.Create(OpCodes.Call, _setValueDependencyPropertyKey)
				: Instruction.Create(OpCodes.Call, _setValue));
			setterInstructions.Add(Instruction.Create(OpCodes.Ret));
		}
	}
}