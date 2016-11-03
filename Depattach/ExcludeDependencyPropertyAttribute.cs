using System;

namespace Depattach
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcludeDependencyPropertyAttribute : Attribute
    {
    }
}