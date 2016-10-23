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

		public static Dictionary<PropertyDefinition, List<Instruction>> InterceptDefaultValuesInConstructor(this TypeDefinition typeDefinition)
		{
			Dictionary<PropertyDefinition, List<Instruction>> results = new Dictionary<PropertyDefinition, List<Instruction>>();

			foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
			{
				
			}

			return results;
		}
	}
}