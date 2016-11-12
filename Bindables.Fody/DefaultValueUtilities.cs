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
		public static Dictionary<PropertyDefinition, Range> InterpretDefaultValues(this TypeDefinition typeDefinition, List<PropertyDefinition> propertiesToConvert)
		{
			// Only auto properties can have initializers.
			// Properties cannot be initialized with instance members.
			// Allowed initializations:
			//   Static methods, properties, fields.
			//   Compile time constants.
			//   Objects created with 'new' keyword.
			// Initializers are injected to all constructors.

			Dictionary<PropertyDefinition, Range> ranges = new Dictionary<PropertyDefinition, Range>();

			foreach (PropertyDefinition propertyDefinition in propertiesToConvert)
			{
				Range range = GetInitializationInstructionRange(typeDefinition, propertyDefinition);
				ranges[propertyDefinition] = range;
			}

			return ranges;
		}

		private static Range GetInitializationInstructionRange(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
		{
			FieldDefinition backingField = typeDefinition.GetBackingFieldForProperty(propertyDefinition);

			if (backingField == null)
			{
				return null;
			}

			// We can use any constructor since the same code is injected into all of them.
			MethodDefinition constructor = typeDefinition.GetConstructors().First();
			List<Instruction> instructions = constructor.Body.Instructions.ToList();

			int startIndexForCurrentProperty = 0;

			for (int i = 0; i < instructions.Count; i++)
			{
				Instruction instruction = instructions[i];

				if (instruction.OpCode == OpCodes.Stfld)
				{
					// A stfld instruction means one of these:
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

		public static void AppendDefaultValueInstructions(this List<Instruction> instructions, TypeDefinition typeDefinition, PropertyDefinition propertyDefinition, Range range)
		{
			MethodDefinition constructor = typeDefinition.GetConstructors().First();

			if (range == null)
			{
				VariableDefinition variableDefinition = instructions.AddInstrunctionForDefaultValue(propertyDefinition);

				if (variableDefinition != null)
				{
					MethodDefinition staticConstructor = typeDefinition.GetStaticConstructor();

					staticConstructor.Body.Variables.Add(variableDefinition);
					staticConstructor.Body.InitLocals = true;
				}

				return;
			}

			List<Instruction> initializationInstructions = constructor.Body.Instructions.GetRange(range);

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

			instructions.Add(propertyDefinition.PropertyType.IsValueType
				? Instruction.Create(OpCodes.Box, propertyDefinition.PropertyType)
				: Instruction.Create(OpCodes.Castclass, typeDefinition.Module.TypeSystem.Object));
		}

		private static VariableDefinition AddInstrunctionForDefaultValue(this List<Instruction> instructions, PropertyDefinition propertyDefinition)
		{
			if (!propertyDefinition.PropertyType.IsValueType)
			{
				instructions.Add(Instruction.Create(OpCodes.Ldnull));
				return null;
			}

			VariableDefinition variable = new VariableDefinition(propertyDefinition.PropertyType);

			instructions.Add(Instruction.Create(OpCodes.Ldloca_S, variable));
			instructions.Add(Instruction.Create(OpCodes.Initobj, propertyDefinition.PropertyType));
			instructions.Add(Instruction.Create(OpCodes.Ldloc, variable));
			instructions.Add(Instruction.Create(OpCodes.Box, propertyDefinition.PropertyType));

			return variable;
		}

		public static void RemoveInitializationInstructionsFromAllConstructors(this TypeDefinition typeDefinition, Dictionary<PropertyDefinition, Range> instructionRanges)
		{
			List<Range> ranges = instructionRanges.Values.Where(range => range != null).ToList();
			ranges.Sort();

			MethodDefinition staticConstructor = typeDefinition.GetStaticConstructor();

			foreach (MethodDefinition constructor in typeDefinition.GetConstructors())
			{
				if (constructor == staticConstructor)
				{
					continue;
				}

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
		}

		private static List<Instruction> GetRange(this Collection<Instruction> instructions, Range range)
		{
			List<Instruction> result = new List<Instruction>();

			for (int i = range.Start; i <= range.End; i++)
			{
				result.Add(instructions[i]);
			}

			return result;
		}
	}
}