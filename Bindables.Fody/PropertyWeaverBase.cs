using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Windows;
using Mono.Cecil.Cil;

namespace Bindables.Fody
{
	public abstract class PropertyWeaverBase
	{
		protected ModuleDefinition ModuleDefinition { get; }

		protected MethodReference GetTypeFromHandle { get; }

		protected MethodReference PropertyChangedCallbackConstructor { get; }

		protected MethodReference FrameworkPropertyMetadataConstructor { get; }
		protected MethodReference FrameworkPropertyMetadataConstructorWithOptions { get; }
		protected MethodReference FrameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback { get; }

		protected MethodReference GetValue { get; }
		protected MethodReference SetValue { get; }

		protected TypeReference DependencyObjectType { get; }
		protected TypeReference DependencyPropertyType { get; }
		protected TypeReference DependencyPropertyChangedEventArgsType { get; }

		protected PropertyWeaverBase(ModuleDefinition moduleDefinition)
		{
			ModuleDefinition = moduleDefinition;

			GetTypeFromHandle = moduleDefinition.ImportMethod(typeof(Type), nameof(Type.GetTypeFromHandle), typeof(RuntimeTypeHandle));

			PropertyChangedCallbackConstructor = moduleDefinition.ImportSingleConstructor(typeof(PropertyChangedCallback));

			FrameworkPropertyMetadataConstructor = moduleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object));
			FrameworkPropertyMetadataConstructorWithOptions = moduleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object), typeof(FrameworkPropertyMetadataOptions));
			FrameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback = moduleDefinition.ImportConstructor(typeof(FrameworkPropertyMetadata), typeof(object), typeof(FrameworkPropertyMetadataOptions), typeof(PropertyChangedCallback));

			GetValue = moduleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.GetValue), typeof(DependencyProperty));
			SetValue = moduleDefinition.ImportMethod(typeof(DependencyObject), nameof(DependencyObject.SetValue), typeof(DependencyProperty), typeof(object));

			DependencyObjectType = moduleDefinition.ImportReference(typeof(DependencyObject));
			DependencyPropertyType = moduleDefinition.ImportReference(typeof(DependencyProperty));
			DependencyPropertyChangedEventArgsType = moduleDefinition.ImportReference(typeof(DependencyPropertyChangedEventArgs));
		}

		public void Execute()
		{
			foreach (TypeDefinition type in ModuleDefinition.Types)
			{
				List<PropertyDefinition> propertiesToConvert = new List<PropertyDefinition>();

				foreach (PropertyDefinition property in type.Properties)
				{
					if (!ShouldConvert(type, property))
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
				ExecuteInternal(type, propertiesToConvert);
				type.RemoveBindableAttributes();
			}
		}

		protected List<Instruction> CreateInstructionsUpToRegistration(TypeDefinition type, PropertyDefinition property, Range instructionRange, CustomAttribute attribute)
		{
			List<Instruction> instructions = new List<Instruction>
			{
				Instruction.Create(OpCodes.Ldstr, property.Name),
				Instruction.Create(OpCodes.Ldtoken, property.PropertyType),
				Instruction.Create(OpCodes.Call, GetTypeFromHandle),
				Instruction.Create(OpCodes.Ldtoken, type),
				Instruction.Create(OpCodes.Call, GetTypeFromHandle)
			};

			AppendDefaultValueInstructions(instructions, type, property, instructionRange);
			
			if (attribute?.HasProperties == true)
			{
				CustomAttributeArgument options = attribute.Properties.FirstOrDefault(p => p.Name == nameof(DependencyPropertyAttribute.Options)).Argument;

				instructions.Add(options.Value == null
					? Instruction.Create(OpCodes.Ldc_I4, (int)FrameworkPropertyMetadataOptions.None)
					: Instruction.Create(OpCodes.Ldc_I4, (int)options.Value));

				string propertyChangedCallback = attribute.Properties.FirstOrDefault(p => p.Name == nameof(DependencyPropertyAttribute.OnPropertyChanged)).Argument.Value as string;
				if (String.IsNullOrEmpty(propertyChangedCallback))
				{
					instructions.Add(Instruction.Create(OpCodes.Newobj, FrameworkPropertyMetadataConstructorWithOptions));
				}
				else
				{
					try
					{
						// If a method is not found, an ArgumentException will be thrown.
						MethodReference method = ModuleDefinition.ImportMethod(type, propertyChangedCallback, DependencyObjectType, DependencyPropertyChangedEventArgsType);

						if (method.HasThis)
						{
							// Found a method with desired signature, but it is not static.
							throw new ArgumentException();
						}

						instructions.Add(Instruction.Create(OpCodes.Ldnull));
						instructions.Add(Instruction.Create(OpCodes.Ldftn, method));
						instructions.Add(Instruction.Create(OpCodes.Newobj, PropertyChangedCallbackConstructor));
						instructions.Add(Instruction.Create(OpCodes.Newobj, FrameworkPropertyMetadataConstructorWithOptionsAndPropertyChangedCallback));
					}
					catch (ArgumentException)
					{
						throw new WeavingException($@"No method with signature: ""static void {propertyChangedCallback}({nameof(DependencyObject)}, {nameof(DependencyPropertyChangedEventArgs)})"" found.")
						{
							SequencePoint = property.GetMethod.DebugInformation.SequencePoints.FirstOrDefault()
						};
					}
				}
			}
			else
			{
				instructions.Add(Instruction.Create(OpCodes.Newobj, FrameworkPropertyMetadataConstructor));
			}

			return instructions;
		}

		protected abstract void ExecuteInternal(TypeDefinition type, List<PropertyDefinition> propertiesToConvert);
		protected abstract void AppendDefaultValueInstructions(List<Instruction> instructions, TypeDefinition type, PropertyDefinition property, Range instructionRange);
		protected abstract bool ShouldConvert(TypeDefinition type, PropertyDefinition property);
	}
}