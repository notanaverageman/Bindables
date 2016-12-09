using System;

namespace Bindables
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcludeAttachedPropertyAttribute : Attribute
    {
    }
}