using System;

namespace Bindables
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcludeDependencyPropertyAttribute : Attribute
    {
    }
}