using System;

namespace Bindables
{
    /// <summary>
    /// Use this attribute to exclude a property from attached property conversion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcludeAttachedPropertyAttribute : Attribute
    {
    }
}