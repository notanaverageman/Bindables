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
	public class DependencyPropertyWeaver
	{
		private readonly ModuleDefinition _moduleDefinition;

		private readonly TypeReference _dependencyPropertyKey;
		private readonly TypeReference _dependencyObject;

		private readonly MethodReference _getTypeFromHandle;

		private readonly MethodReference _registerDependencyProperty;
		private readonly MethodReference _registerDependencyPropertyReadOnly;

		private readonly MethodReference _setValue;
		private readonly MethodReference _setValueDependencyPropertyKey;
		private readonly MethodReference _getValue;
		private readonly MethodReference _getDependencyProperty;

		private readonly MethodReference _propertyChangedCallbackConstructor;

		private readonly MethodReference _frameworkPropertyMetadataConstructor;
		private readonly MethodReference _frameworkPropertyMetadataConstructorWithOptions;
		private readonly MethodReference _frameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback;

		public DependencyPropertyWeaver(ModuleDefinition moduleDefinition)
		{
			_moduleDefinition = moduleDefinition;

			_dependencyPropertyKey = moduleDefinition.ImportReference(typeof(DependencyPropertyKey));
			_dependencyObject = moduleDefinition.ImportReference(typeof(DependencyObject));

			_getTypeFromHandle = moduleDefinition.ImportMethod(typeof(Type), nameof(Type.GetTypeFromHandle), typeof(RuntimeTypeHandle));

			_registerDependencyProperty = moduleDefinition.ImportMethod(typeof(DependencyProperty), nameof(DependencyProperty.Register), typeof(string), typeof(Type), typeof(Type), typeof(PropertyMetadata));
			_registerDependencyPropertyReadOnly = moduleDefinition.ImportMethod(typeof(DependencyProperty), nameof(DependencyProperty.RegisterReadOnly), typeof(string), typeof(Type), typeof(Type), typeof(PropertyMetadata));

			_setValue = moduleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.SetValue), typeof(DependencyProperty), typeof(object));
			_setValueDependencyPropertyKey = moduleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.SetValue), typeof(DependencyPropertyKey), typeof(object));
			_getValue = moduleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.GetValue), typeof(DependencyProperty));
			_getDependencyProperty = moduleDefinition.ImportMethod(typeof(DependencyPropertyKey), $"get_{nameof(DependencyPropertyKey.DependencyProperty)}");

			_propertyChangedCallbackConstructor = moduleDefinition.ImportSingleConstructor(typeof(PropertyChangedCallback));

			_frameworkPropertyMetadataConstructor = moduleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object));
			_frameworkPropertyMetadataConstructorWithOptions = moduleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object), typeof(FrameworkPropertyMetadataOptions));
			_frameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback = moduleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object), typeof(FrameworkPropertyMetadataOptions), typeof(PropertyChangedCallback));
		}

		public void Execute()
		{
			foreach (TypeDefinition type in _moduleDefinition.Types)
			{
				List<PropertyDefinition> propertiesToConvert = new List<PropertyDefinition>();

				foreach (PropertyDefinition property in type.Properties)
				{
					if (!ShouldConvertToDependencyProperty(type, property))
					{
						continue;
					}

					propertiesToConvert.Add(property);
				}

				if (!propertiesToConvert.Any())
				{
					continue;
				}

				type.CreateStaticConstructorIfNotExists();

				Dictionary<PropertyDefinition, Range> instructionRanges = type.InterpretDefaultValues(propertiesToConvert);

				foreach (PropertyDefinition property in propertiesToConvert)
				{
					property.ValidateBeforeConversion(type);
					type.ValidateBeforeConversion(_dependencyObject);

					Range instructionRange = instructionRanges[property];
					ConvertToDependencyProperty(type, property, instructionRange);

					property.RemoveDependencyPropertyAttribute();
				}

				type.RemoveInitializationInstructionsFromAllConstructors(instructionRanges);
				type.RemoveDependencyPropertyAttribute();
			}
		}

		private bool ShouldConvertToDependencyProperty(TypeDefinition type, PropertyDefinition property)
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

				if (property.GetMethod == null || property.SetMethod == null)
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
			FieldDefinition backingField = type.GetBackingFieldForProperty(property);

			string fieldName = property.Name + "Property";
			FieldDefinition field = new FieldDefinition(fieldName, FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Public, _dependencyObject);

			type.Fields.Add(field);

			if (property.IsReadOnly())
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

			type.Fields.Remove(backingField);
		}

		private void AddInitializationToStaticConstructor(TypeDefinition type, PropertyDefinition property, FieldDefinition field, Range instructionRange)
		{
			MethodDefinition staticConstructor = type.GetStaticConstructor();

			List<Instruction> instructions = CreateInstructionsUpToRegistration(type, property, instructionRange);

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

			List<Instruction> instructions = CreateInstructionsUpToRegistration(type, property, instructionRange);

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

		private List<Instruction> CreateInstructionsUpToRegistration(TypeDefinition type, PropertyDefinition property, Range instructionRange)
		{
			List<Instruction> instructions = new List<Instruction>
			{
				Instruction.Create(OpCodes.Ldstr, property.Name),
				Instruction.Create(OpCodes.Ldtoken, property.PropertyType),
				Instruction.Create(OpCodes.Call, _getTypeFromHandle),
				Instruction.Create(OpCodes.Ldtoken, type),
				Instruction.Create(OpCodes.Call, _getTypeFromHandle)
			};

			instructions.AppendDefaultValueInstructions(type, property, instructionRange);

			CustomAttribute dependencyPropertyAttribute = property.GetDependencyPropertyAttribute();
			if (dependencyPropertyAttribute?.HasProperties == true)
			{
				CustomAttributeArgument options = dependencyPropertyAttribute.Properties.FirstOrDefault(p => p.Name == nameof(DependencyPropertyAttribute.Options)).Argument;

				instructions.Add(options.Value == null
					? Instruction.Create(OpCodes.Ldc_I4, (int)FrameworkPropertyMetadataOptions.None)
					: Instruction.Create(OpCodes.Ldc_I4, (int)options.Value));

				string propertyChangedCallback = dependencyPropertyAttribute.Properties.FirstOrDefault(p => p.Name == nameof(DependencyPropertyAttribute.OnPropertyChanged)).Argument.Value as string;
				if (String.IsNullOrEmpty(propertyChangedCallback))
				{
					instructions.Add(Instruction.Create(OpCodes.Newobj, _frameworkPropertyMetadataConstructorWithOptions));
				}
				else
				{
					try
					{
						MethodReference method = _moduleDefinition.ImportMethod(type, propertyChangedCallback, typeof(DependencyObject), typeof(DependencyPropertyChangedEventArgs));
						if (method.HasThis)
						{
							// Found a method with desired signature, but it is not static.
							throw new ArgumentException();
						}

						instructions.Add(Instruction.Create(OpCodes.Ldnull));
						instructions.Add(Instruction.Create(OpCodes.Ldftn, method));
						instructions.Add(Instruction.Create(OpCodes.Newobj, _propertyChangedCallbackConstructor));
						instructions.Add(Instruction.Create(OpCodes.Newobj, _frameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback));
					}
					catch (ArgumentException)
					{
						throw new WeavingException($@"No method with signature: ""void {propertyChangedCallback}({nameof(DependencyObject)}, {nameof(DependencyPropertyChangedEventArgs)})"" found.")
						{
							SequencePoint = property.GetMethod.Body.Instructions.First().SequencePoint
						};
					}
				}
			}
			else
			{
				instructions.Add(Instruction.Create(OpCodes.Newobj, _frameworkPropertyMetadataConstructor));
			}

			return instructions;
		}

		private void ModifyGetMethod(PropertyDefinition property, FieldDefinition field)
		{
			MethodDefinition getter = property.GetMethod;
			Collection<Instruction> getterInstructions = getter.Body.Instructions;

			getterInstructions.Clear();
			getterInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			getterInstructions.Add(Instruction.Create(OpCodes.Ldsfld, field));
			getterInstructions.Add(Instruction.Create(OpCodes.Call, _getValue));
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
				: Instruction.Create(OpCodes.Call, _setValue));
			setterInstructions.Add(Instruction.Create(OpCodes.Ret));
		}
	}
}