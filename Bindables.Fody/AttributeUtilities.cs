using System;
using System.Linq;
using System.Windows;
using Mono.Cecil;

namespace Bindables.Fody
{
	public static class AttributeUtilities
	{
		public static CustomAttribute GetDependencyPropertyAttribute(this IMemberDefinition memberDefinition)
		{
			return memberDefinition.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == typeof(DependencyPropertyAttribute).FullName);
		}

		public static void RemoveDependencyPropertyAttribute(this IMemberDefinition memberDefinition)
		{
			CustomAttribute attribute = memberDefinition.GetDependencyPropertyAttribute();

			if (attribute != null)
			{
				memberDefinition.CustomAttributes.Remove(attribute);
			}
		}

		public static bool ShouldConvertToDependencyProperty(this PropertyDefinition propertyDefinition, TypeDefinition typeDefinition)
		{
			CustomAttribute typeAttribute = typeDefinition.GetDependencyPropertyAttribute();
			CustomAttribute propertyAttribute = propertyDefinition.GetDependencyPropertyAttribute();

			FieldDefinition backingField = typeDefinition.GetBackingFieldForProperty(propertyDefinition);

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

				if (propertyDefinition.GetMethod == null || propertyDefinition.SetMethod == null)
				{
					return false;
				}

			    if (propertyDefinition.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(ExcludeDependencyPropertyAttribute).FullName))
			    {
			        return false;
			    }
			}

			return true;
		}

		public static void ValidateBeforeConversion(this PropertyDefinition propertyDefinition, TypeDefinition typeDefinition)
		{
			CustomAttribute attribute = propertyDefinition.GetDependencyPropertyAttribute();
			FieldDefinition backingField = typeDefinition.GetBackingFieldForProperty(propertyDefinition);

			if (attribute != null && backingField == null)
			{
				throw new InvalidOperationException("Cannot convert to dependency property because the property does not have a backing field.");
			}

			if (propertyDefinition.GetMethod == null || propertyDefinition.SetMethod == null)
			{
				throw new InvalidOperationException("Cannot convert to dependency property because the property is read-only.");
			}
		}

		public static void ValidateBeforeConversion(this TypeDefinition typeDefinition, TypeReference dependencyObject)
		{
			if (!typeDefinition.InheritsFrom(dependencyObject))
			{
				throw new InvalidOperationException($"Your class should inherit from {typeof(DependencyObject)} to be able to define dependency properties.");
			}
		}
	}
}