using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Bindables.Fody
{
	public static class DefaultValueUtilities
	{
		// Only auto properties can have initializers.
		// Properties cannot be initialized with instance members.
		// Allowed initializations:
		//   Static methods, properties, fields.
		//   Compile time constants.
		//   Objects created with 'new' keyword.
		// Initializers are injected to all constructors.

		public static Dictionary<PropertyDefinition, Range> InterpretDefaultValues(this TypeDefinition type, List<PropertyDefinition> propertiesToConvert)
		{
			Dictionary<PropertyDefinition, Range> instructionRanges = new Dictionary<PropertyDefinition, Range>();

			foreach (PropertyDefinition property in propertiesToConvert)
			{
				Range instructionRange = GetInitializationInstructionsRange(type, property, false);
				instructionRanges[property] = instructionRange;
			}

			return instructionRanges;
		}

		public static Dictionary<PropertyDefinition, Range> InterpretDefaultValuesStatic(this TypeDefinition type, List<PropertyDefinition> propertiesToConvert)
		{
			Dictionary<PropertyDefinition, Range> instructionRanges = new Dictionary<PropertyDefinition, Range>();

			foreach (PropertyDefinition property in propertiesToConvert)
			{
				Range instructionRange = GetInitializationInstructionsRange(type, property, true);
				instructionRanges[property] = instructionRange;
			}

			return instructionRanges;
		}

		private static Range GetInitializationInstructionsRange(TypeDefinition type, PropertyDefinition property, bool lookInStaticConstructor)
		{
			FieldDefinition backingField = type.GetBackingFieldForProperty(property);

			if (backingField == null)
			{
				return null;
			}

			// For non-static constructors we can use any one of them, since the same code is injected into the all constructors.
			MethodDefinition constructor = lookInStaticConstructor ? type.GetStaticConstructor() : type.GetConstructors().First();
			List<Instruction> instructions = constructor.Body.Instructions.ToList();

			return GetInitializationInstructionsRange(instructions, backingField, lookInStaticConstructor ? OpCodes.Stsfld : OpCodes.Stfld);
		}

		private static Range GetInitializationInstructionsRange(List<Instruction> constructorInstructions, FieldDefinition backingField, OpCode opCodeToLookFor)
		{
			int startIndexForCurrentProperty = 0;

			for (int i = 0; i < constructorInstructions.Count; i++)
			{
				Instruction instruction = constructorInstructions[i];

				if (instruction.OpCode == opCodeToLookFor)
				{
					// A stfld or stsfld instruction means one of these:
					//   Initializer for a field.
					//   Initializer for a property. (It might be our property.)
					//   An explicit statement that sets a field in constructor. (Always after initialization instructions.)

					if (instruction.Operand == backingField)
					{
						// We have found the instruction that sets our backing field.
						// The instructions we want are between 'startIndexForCurrentProperty' and 'i'.
						return new Range(startIndexForCurrentProperty, i);
					}
					else
					{
						// This implies that if there is a 'next' instruction it might be the first instruction of a our initialization.
						startIndexForCurrentProperty = i + 1;
					}
				}
			}

			return null;
		}

		public static void AppendDefaultValueInstructionsForDependencyProperty(this List<Instruction> instructions, TypeDefinition type, PropertyDefinition property, Range instructionRange)
		{
			MethodDefinition constructor = type.GetConstructors().First(c => !c.IsStatic);

			if (instructionRange == null)
			{
				VariableDefinition variable = instructions.AddInstrunctionForDefaultValue(property);

				if (variable != null)
				{
					MethodDefinition staticConstructor = type.GetStaticConstructor();

					staticConstructor.Body.Variables.Add(variable);
					staticConstructor.Body.InitLocals = true;
				}

				return;
			}

			List<Instruction> initializationInstructions = constructor.Body.Instructions.GetRange(instructionRange);

			// The last instruction should be stfld with operand as our backing field.
			// Remove it as we will get rid of the backing field and just use the default value.
			initializationInstructions.RemoveAt(initializationInstructions.Count - 1);

			// Remove the first instruction. It is a ldarg0 instruction which loads 'this' in a constructor.
			// It is meaningless in static constructor.
			initializationInstructions.RemoveAt(0);

			foreach (Instruction instruction in initializationInstructions)
			{
				instructions.Add(instruction);
			}

			instructions.Add(property.PropertyType.IsValueType
				? Instruction.Create(OpCodes.Box, property.PropertyType)
				: Instruction.Create(OpCodes.Castclass, type.Module.TypeSystem.Object));
		}

		public static void AppendDefaultValueInstructionsForAttachedProperty(this List<Instruction> instructions, TypeDefinition type, PropertyDefinition property, Range instructionRange)
		{
			MethodDefinition constructor = type.GetStaticConstructor();

			if (instructionRange == null)
			{
				VariableDefinition variable = instructions.AddInstrunctionForDefaultValue(property);

				if (variable != null)
				{
					MethodDefinition staticConstructor = type.GetStaticConstructor();

					staticConstructor.Body.Variables.Add(variable);
					staticConstructor.Body.InitLocals = true;
				}

				return;
			}

			List<Instruction> initializationInstructions = constructor.Body.Instructions.GetRange(instructionRange);

			// The last instruction should be stfld with operand as our backing field.
			// Remove it as we will get rid of the backing field and just use the default value.
			initializationInstructions.RemoveAt(initializationInstructions.Count - 1);
			
			foreach (Instruction instruction in initializationInstructions)
			{
				instructions.Add(instruction);
			}

			instructions.Add(property.PropertyType.IsValueType
				? Instruction.Create(OpCodes.Box, property.PropertyType)
				: Instruction.Create(OpCodes.Castclass, type.Module.TypeSystem.Object));
		}

		private static VariableDefinition AddInstrunctionForDefaultValue(this List<Instruction> instructions, PropertyDefinition property)
		{
			if (!property.PropertyType.IsValueType)
			{
				instructions.Add(Instruction.Create(OpCodes.Ldnull));
				return null;
			}

			VariableDefinition variable = new VariableDefinition(property.PropertyType);

			instructions.Add(Instruction.Create(OpCodes.Ldloca_S, variable));
			instructions.Add(Instruction.Create(OpCodes.Initobj, property.PropertyType));
			instructions.Add(Instruction.Create(OpCodes.Ldloc, variable));
			instructions.Add(Instruction.Create(OpCodes.Box, property.PropertyType));

			return variable;
		}

		public static void RemoveInitializationInstructionsFromConstructor(this MethodDefinition constructor, Dictionary<PropertyDefinition, Range> instructionRanges)
		{
			List<Range> ranges = instructionRanges.Values.Where(range => range != null).ToList();
			ranges.Sort();

			Collection<Instruction> instructions = constructor.Body.Instructions;

			for (int i = ranges.Count - 1; i >= 0; i--)
			{
				Range range = ranges[i];

				for (int j = range.End; j >= range.Start; j--)
				{
					instructions.RemoveAt(j);
				}
			}
		}

		private static List<Instruction> GetRange(this Collection<Instruction> instructions, Range instructionRange)
		{
			List<Instruction> result = new List<Instruction>();

			for (int i = instructionRange.Start; i <= instructionRange.End; i++)
			{
				result.Add(instructions[i]);
			}

			return result;
		}
	}
}