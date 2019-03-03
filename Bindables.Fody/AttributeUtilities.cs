using System.Linq;
using System.Windows;
using Mono.Cecil;

namespace Bindables.Fody
{
	public static class AttributeUtilities
	{

		public static CustomAttribute GetDependencyPropertyAttribute(this IMemberDefinition member)
		{
			return member.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == Consts.DependencyPropertyAttribute);
		}

		public static CustomAttribute GetAttachedPropertyAttribute(this IMemberDefinition member)
		{
			return member.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == Consts.AttachedPropertyAttribute);
		}

		public static void RemoveBindableAttributes(this IMemberDefinition member)
		{
			CustomAttribute dependencyPropertyAttribute = member.GetDependencyPropertyAttribute();
			CustomAttribute attachedPropertyAttribute = member.GetAttachedPropertyAttribute();

			if (dependencyPropertyAttribute != null)
			{
				member.CustomAttributes.Remove(dependencyPropertyAttribute);
			}

			if (attachedPropertyAttribute != null)
			{
				member.CustomAttributes.Remove(attachedPropertyAttribute);
			}
		}

		public static void ValidateBeforeDependencyPropertyConversion(this PropertyDefinition property, TypeDefinition type)
		{
			CustomAttribute attribute = property.GetDependencyPropertyAttribute();
			FieldDefinition backingField = type.GetBackingFieldForProperty(property);

			if (attribute != null && backingField == null)
			{
				throw new WeavingException("Cannot convert to dependency property because the property does not have a backing field.");
			}

			if (property.GetMethod == null || property.SetMethod == null)
			{
				throw new WeavingException("Cannot convert to dependency property because the property is not an auto property.");
			}
		}

		public static void ValidateBeforeDependencyPropertyConversion(this TypeDefinition type, TypeReference dependencyObject)
		{
			if (!type.InheritsFrom(dependencyObject))
			{
				throw new WeavingException($"Your class should inherit from {typeof(DependencyObject)} to be able to define dependency properties.");
			}
		}

		public static void ValidateBeforeAttachedPropertyConversion(this PropertyDefinition property, TypeDefinition type)
		{
			CustomAttribute attribute = property.GetAttachedPropertyAttribute();
			FieldDefinition backingField = type.GetBackingFieldForProperty(property);

			if (attribute != null && backingField == null)
			{
				throw new WeavingException("Cannot convert to attached property because the property does not have a backing field.");
			}

			if (property.GetMethod == null || property.SetMethod == null)
			{
				throw new WeavingException("Cannot convert to attached property because the property is not an auto property.");
			}

			if (!property.GetMethod.IsStatic)
			{
				throw new WeavingException("Cannot convert to attached property because the property is not static.");
			}
		}

		public static bool IsMarkedAsReadOnly(this PropertyDefinition property)
		{
			CustomAttribute attribute = property.GetDependencyPropertyAttribute();
			bool? isReadonly = attribute?.Properties.FirstOrDefault(p => p.Name == Consts.IsReadOnly).Argument.Value as bool?;

			return isReadonly == true;
		}
	}
}