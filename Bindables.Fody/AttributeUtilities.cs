using System.Linq;
using System.Windows;
using Mono.Cecil;

namespace Bindables.Fody
{
	public static class AttributeUtilities
	{
		public static CustomAttribute GetDependencyPropertyAttribute(this IMemberDefinition member)
		{
			return member.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == nameof(DependencyPropertyAttribute));
		}

		public static void RemoveDependencyPropertyAttribute(this IMemberDefinition member)
		{
			CustomAttribute attribute = member.GetDependencyPropertyAttribute();

			if (attribute != null)
			{
				member.CustomAttributes.Remove(attribute);
			}
		}

		public static void ValidateBeforeConversion(this PropertyDefinition propertyn, TypeDefinition type)
		{
			CustomAttribute attribute = propertyn.GetDependencyPropertyAttribute();
			FieldDefinition backingField = type.GetBackingFieldForProperty(propertyn);

			if (attribute != null && backingField == null)
			{
				throw new WeavingException("Cannot convert to dependency property because the property does not have a backing field.");
			}

			if (propertyn.GetMethod == null || propertyn.SetMethod == null)
			{
				throw new WeavingException("Cannot convert to dependency property because the property is not an auto property.");
			}
		}

		public static void ValidateBeforeConversion(this TypeDefinition type, TypeReference dependencyObject)
		{
			if (!type.InheritsFrom(dependencyObject))
			{
				throw new WeavingException($"Your class should inherit from {typeof(DependencyObject)} to be able to define dependency properties.");
			}
		}

		public static bool IsReadOnly(this PropertyDefinition property)
		{
			CustomAttribute attribute = property.GetDependencyPropertyAttribute();
			bool? isReadonly = attribute?.Properties.FirstOrDefault(p => p.Name == nameof(DependencyPropertyAttribute.IsReadOnly)).Argument.Value as bool?;

			return isReadonly == true;
		}
	}
}