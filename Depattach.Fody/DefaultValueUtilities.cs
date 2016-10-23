using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Depattach.Fody
{
	public static class DefaultValueUtilities
	{
		public static VariableDefinition AddInstrunctionForDefaultValue(this List<Instruction> instructions, PropertyDefinition propertyDefinition)
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

		public static void AddInstrunctionForDefaultValue(this List<Instruction> instructions, bool defaultValue, ModuleDefinition moduleDefinition)
		{
			instructions.Add(Instruction.Create(OpCodes.Ldc_I4, defaultValue ? 1 : 0));
			instructions.Add(Instruction.Create(OpCodes.Box, moduleDefinition.TypeSystem.Boolean));
		}

		public static void AddInstrunctionForDefaultValue(this List<Instruction> instructions, byte defaultValue, ModuleDefinition moduleDefinition)
		{
			instructions.Add(Instruction.Create(OpCodes.Ldc_I4, defaultValue));
			instructions.Add(Instruction.Create(OpCodes.Box, moduleDefinition.TypeSystem.Byte));
		}

		public static void AddInstrunctionForDefaultValue(this List<Instruction> instructions, short defaultValue, ModuleDefinition moduleDefinition)
		{
			instructions.Add(Instruction.Create(OpCodes.Ldc_I4, defaultValue));
			instructions.Add(Instruction.Create(OpCodes.Box, moduleDefinition.TypeSystem.Int16));
		}

		public static void AddInstrunctionForDefaultValue(this List<Instruction> instructions, int defaultValue, ModuleDefinition moduleDefinition)
		{
			instructions.Add(Instruction.Create(OpCodes.Ldc_I4, defaultValue));
			instructions.Add(Instruction.Create(OpCodes.Box, moduleDefinition.TypeSystem.Int32));
		}

		public static void AddInstrunctionForDefaultValue(this List<Instruction> instructions, string defaultValue)
		{
			instructions.Add(Instruction.Create(OpCodes.Ldstr, defaultValue));
		}
	}
}