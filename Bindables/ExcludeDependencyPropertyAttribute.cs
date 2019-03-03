using System;

namespace Bindables
{
	/// <summary>
	/// Use this attribute to exclude a property from dependency property conversion.
	/// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcludeDependencyPropertyAttribute : Attribute
    {
    }
}