using NUnit.Framework;

namespace Bindables.Xamarin.Test;

public abstract partial class XamarinDependencyPropertyTests<T>
{
	[Test]
	public void RegularProperty()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class ExampleClass : BaseClassName
{
    private static readonly string DefaultValue = ""Test"";

	[AttributeName(typeof(int))]
	public static readonly PropertyType Example1Property;

	[AttributeName(typeof(int), OnPropertyChanged = nameof(PropertyChangedCallback))]
	public static readonly PropertyType Example2Property;

	[AttributeName(typeof(string), DefaultValueField = nameof(DefaultValue))]
	public static readonly PropertyType Example3Property;

	[AttributeName(typeof(string), BindingMode = BindingMode.OneTime)]
	public static readonly PropertyType Example4Property;

	[AttributeName(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallback), DefaultValueField = nameof(DefaultValue))]
	public static readonly PropertyType Example5Property;

	[AttributeName(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallback), DefaultValueField = nameof(DefaultValue), BindingMode = BindingMode.OneTime)]
	public static readonly PropertyType Example6Property;

    private static void PropertyChangedCallback(BindableObject obj, object oldValue, object newValue)
    {
    }
}";

		const string expectedSourceCodeTemplate = @"
// Generated by Bindables
using PlatformNamespace;

#nullable enable

public partial class ExampleClass
{
    public int Example1
    {
        get => (int)GetValue(Example1Property);
        set => SetValue(Example1Property, value);
    }

    public int Example2
    {
        get => (int)GetValue(Example2Property);
        set => SetValue(Example2Property, value);
    }

    public string? Example3
    {
        get => (string?)GetValue(Example3Property);
        set => SetValue(Example3Property, value);
    }

    public string? Example4
    {
        get => (string?)GetValue(Example4Property);
        set => SetValue(Example4Property, value);
    }

    public string? Example5
    {
        get => (string?)GetValue(Example5Property);
        set => SetValue(Example5Property, value);
    }

    public string? Example6
    {
        get => (string?)GetValue(Example6Property);
        set => SetValue(Example6Property, value);
    }

    static ExampleClass()
    {
        Example1Property = PropertyType.Create(
            ""Example1"",
            typeof(int),
            typeof(ExampleClass),
            default);
        
        Example2Property = PropertyType.Create(
            ""Example2"",
            typeof(int),
            typeof(ExampleClass),
            default,
            propertyChanged: PropertyChangedCallback);
        
        Example3Property = PropertyType.Create(
            ""Example3"",
            typeof(string),
            typeof(ExampleClass),
            DefaultValue);
        
        Example4Property = PropertyType.Create(
            ""Example4"",
            typeof(string),
            typeof(ExampleClass),
            default,
            defaultBindingMode: (BindingMode)4);
        
        Example5Property = PropertyType.Create(
            ""Example5"",
            typeof(string),
            typeof(ExampleClass),
            DefaultValue,
            propertyChanged: PropertyChangedCallback);
        
        Example6Property = PropertyType.Create(
            ""Example6"",
            typeof(string),
            typeof(ExampleClass),
            DefaultValue,
            defaultBindingMode: (BindingMode)4,
            propertyChanged: PropertyChangedCallback);
        
    }
}";

		TestSourceCodeTemplate(sourceCodeTemplate, expectedSourceCodeTemplate);
	}
}
