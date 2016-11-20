using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Bindables.Fody
{
	internal static class AssemblyUtilities
	{
		public static void CreateStaticConstructorIfNotExists(this TypeDefinition type)
		{
			MethodDefinition staticConstructor = type.GetStaticConstructor();
			if (staticConstructor != null)
			{
				return;
			}

			staticConstructor = new MethodDefinition(
										".cctor",
										MethodAttributes.Private |
										MethodAttributes.HideBySig |
										MethodAttributes.SpecialName |
										MethodAttributes.RTSpecialName |
										MethodAttributes.Static,
										type.Module.TypeSystem.Void
									);

			staticConstructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
			type.Methods.Add(staticConstructor);
		}

		public static bool InheritsFrom(this TypeDefinition derivedType, TypeReference baseType)
		{
			if (derivedType == null || baseType == null)
			{
				return false;
			}

			if (derivedType.FullName == baseType.FullName)
			{
				return true;
			}

			return InheritsFrom(derivedType.BaseType?.Resolve(), baseType);
		}

		public static MethodReference ImportConstructor(this ModuleDefinition moduleDefinition, Type type, params Type[] parameterTypes)
		{
			TypeReference typeReference = moduleDefinition.ImportReference(type);
			IEnumerable<MethodReference> constructors = typeReference.Resolve().GetConstructors();

			return GetMethodReference(moduleDefinition, constructors, parameterTypes);
		}

		public static MethodReference ImportSingleConstructor(this ModuleDefinition moduleDefinition, Type type)
		{
			TypeReference typeReference = moduleDefinition.ImportReference(type);
			MethodReference constructor = typeReference.Resolve().GetConstructors().Single();

			return moduleDefinition.ImportReference(constructor);
		}

		public static MethodReference ImportMethod(this ModuleDefinition moduleDefinition, Type type, string methodName, params Type[] parameterTypes)
		{
			TypeDefinition typeDefinition = moduleDefinition.ImportReference(type).Resolve();
			return ImportMethod(moduleDefinition, typeDefinition, methodName, parameterTypes);
		}

		public static MethodReference ImportMethod(this ModuleDefinition moduleDefinition, TypeDefinition type, string methodName, params Type[] parameterTypes)
		{
			IEnumerable<MethodReference> methods = type.Methods.Where(m => m.Name == methodName);
			return GetMethodReference(moduleDefinition, methods, parameterTypes);
		}

		private static MethodReference GetMethodReference(ModuleDefinition moduleDefinition, IEnumerable<MethodReference> methods, Type[] parameterTypes)
		{
			foreach (MethodReference methodReference in methods)
			{
				Collection<ParameterDefinition> parameters = methodReference.Parameters;

				if (parameters.Count != parameterTypes.Length)
				{
					continue;
				}

				bool allSame = parameters.Count == parameterTypes.Length;

				for (int i = 0; i < parameters.Count; i++)
				{
					ParameterDefinition parameter = parameters[i];
					Type parameterType = parameterTypes[i];

					if (parameter.ParameterType.FullName != parameterType.FullName)
					{
						allSame = false;
					}
				}

				if (allSame)
				{
					return moduleDefinition.ImportReference(methodReference);
				}
			}

			throw new ArgumentException("No methods found.");
		}

		public static FieldDefinition GetBackingFieldForProperty(this TypeDefinition type, PropertyDefinition property)
		{
			return type.Fields.FirstOrDefault(f => f.Name == $"<{property.Name}>k__BackingField" && f.FieldType.FullName == property.PropertyType.FullName);
		}
	}
}