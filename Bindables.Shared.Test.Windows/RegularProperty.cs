using NUnit.Framework;

namespace Bindables.Windows.Test;

public abstract partial class WindowsTests<T>
{
	[Test]
	public void RegularProperty()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class ExampleClass : {BaseClassName}
{{
    private static readonly string DefaultValue = ""Test"";

    [{DependencyPropertyAttributeName}(typeof(int))]
	public static readonly {DependencyPropertyName} Example1Property;

	[{DependencyPropertyAttributeName}(typeof(int), OnPropertyChanged = nameof(PropertyChangedCallback))]
	public static readonly {DependencyPropertyName} Example2Property;

	[{DependencyPropertyAttributeName}(typeof(string), DefaultValueField = nameof(DefaultValue))]
	public static readonly {DependencyPropertyName} Example3Property;

	[{DependencyPropertyAttributeName}(typeof(string), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static readonly {DependencyPropertyName} Example4Property;

	[{DependencyPropertyAttributeName}(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallback), DefaultValueField = nameof(DefaultValue))]
	public static readonly {DependencyPropertyName} Example5Property;

	[{DependencyPropertyAttributeName}(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallback), DefaultValueField = nameof(DefaultValue), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static readonly {DependencyPropertyName} Example6Property;

	[{DependencyPropertyAttributeName}(typeof(int), OnCoerceValue = nameof(CoerceValueCallback))]
	public static readonly {DependencyPropertyName} Example7Property;

	[{DependencyPropertyAttributeName}(typeof(int), OnPropertyChanged = nameof(PropertyChangedCallback), OnCoerceValue = nameof(CoerceValueCallback))]
	public static readonly {DependencyPropertyName} Example8Property;

	[{DependencyPropertyAttributeName}(typeof(string), OnCoerceValue = nameof(CoerceValueCallback), DefaultValueField = nameof(DefaultValue))]
	public static readonly {DependencyPropertyName} Example9Property;

	[{DependencyPropertyAttributeName}(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallback), OnCoerceValue = nameof(CoerceValueCallback), DefaultValueField = nameof(DefaultValue))]
	public static readonly {DependencyPropertyName} Example10Property;

	[{DependencyPropertyAttributeName}(typeof(string), OnCoerceValue = nameof(CoerceValueCallback), DefaultValueField = nameof(DefaultValue), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static readonly {DependencyPropertyName} Example11Property;

	[{DependencyPropertyAttributeName}(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallback), OnCoerceValue = nameof(CoerceValueCallback), DefaultValueField = nameof(DefaultValue), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static readonly {DependencyPropertyName} Example12Property;

    private static void PropertyChangedCallback({BaseClassName} obj, DependencyPropertyChangedEventArgs args)
    {{
    }}

	private static object CoerceValueCallback({BaseClassName} obj, object value)
	{{
		return """";
	}}

    // ATTACHED

    private static readonly string DefaultValueAttached = ""Test"";

	[{AttachedPropertyAttributeName}(typeof(int))]
	public static readonly {DependencyPropertyName} ExampleAttached1Property;

	[{AttachedPropertyAttributeName}(typeof(int), OnPropertyChanged = nameof(PropertyChangedCallbackAttached))]
	public static readonly {DependencyPropertyName} ExampleAttached2Property;

	[{AttachedPropertyAttributeName}(typeof(string), DefaultValueField = nameof(DefaultValueAttached))]
	public static readonly {DependencyPropertyName} ExampleAttached3Property;

	[{AttachedPropertyAttributeName}(typeof(string), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static readonly {DependencyPropertyName} ExampleAttached4Property;

	[{AttachedPropertyAttributeName}(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallbackAttached), DefaultValueField = nameof(DefaultValueAttached))]
	public static readonly {DependencyPropertyName} ExampleAttached5Property;

	[{AttachedPropertyAttributeName}(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallbackAttached), DefaultValueField = nameof(DefaultValueAttached), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static readonly {DependencyPropertyName} ExampleAttached6Property;

	[{AttachedPropertyAttributeName}(typeof(int), OnCoerceValue = nameof(CoerceValueCallbackAttached))]
	public static readonly {DependencyPropertyName} ExampleAttached7Property;

	[{AttachedPropertyAttributeName}(typeof(int), OnPropertyChanged = nameof(PropertyChangedCallbackAttached), OnCoerceValue = nameof(CoerceValueCallbackAttached))]
	public static readonly {DependencyPropertyName} ExampleAttached8Property;

	[{AttachedPropertyAttributeName}(typeof(string), OnCoerceValue = nameof(CoerceValueCallbackAttached), DefaultValueField = nameof(DefaultValueAttached))]
	public static readonly {DependencyPropertyName} ExampleAttached9Property;

	[{AttachedPropertyAttributeName}(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallbackAttached), OnCoerceValue = nameof(CoerceValueCallbackAttached), DefaultValueField = nameof(DefaultValueAttached))]
	public static readonly {DependencyPropertyName} ExampleAttached10Property;

	[{AttachedPropertyAttributeName}(typeof(string), OnCoerceValue = nameof(CoerceValueCallbackAttached), DefaultValueField = nameof(DefaultValueAttached), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static readonly {DependencyPropertyName} ExampleAttached11Property;

	[{AttachedPropertyAttributeName}(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallbackAttached), OnCoerceValue = nameof(CoerceValueCallbackAttached), DefaultValueField = nameof(DefaultValueAttached), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static readonly {DependencyPropertyName} ExampleAttached12Property;

	private static void PropertyChangedCallbackAttached({BaseClassName} obj, DependencyPropertyChangedEventArgs args)
	{{
	}}

	private static object CoerceValueCallbackAttached({BaseClassName} obj, object value)
	{{
		return """";
	}}
}}";

		string expectedSourceCode = $@"
// Generated by Bindables
using {PlatformNamespace};

#nullable enable

public partial class ExampleClass
{{
    public int Example1
    {{
        get => (int)GetValue(Example1Property);
        set => SetValue(Example1Property, value);
    }}

    public int Example2
    {{
        get => (int)GetValue(Example2Property);
        set => SetValue(Example2Property, value);
    }}

    public string? Example3
    {{
        get => (string?)GetValue(Example3Property);
        set => SetValue(Example3Property, value);
    }}

    public string? Example4
    {{
        get => (string?)GetValue(Example4Property);
        set => SetValue(Example4Property, value);
    }}

    public string? Example5
    {{
        get => (string?)GetValue(Example5Property);
        set => SetValue(Example5Property, value);
    }}

    public string? Example6
    {{
        get => (string?)GetValue(Example6Property);
        set => SetValue(Example6Property, value);
    }}

    public int Example7
    {{
        get => (int)GetValue(Example7Property);
        set => SetValue(Example7Property, value);
    }}

    public int Example8
    {{
        get => (int)GetValue(Example8Property);
        set => SetValue(Example8Property, value);
    }}

    public string? Example9
    {{
        get => (string?)GetValue(Example9Property);
        set => SetValue(Example9Property, value);
    }}

    public string? Example10
    {{
        get => (string?)GetValue(Example10Property);
        set => SetValue(Example10Property, value);
    }}

    public string? Example11
    {{
        get => (string?)GetValue(Example11Property);
        set => SetValue(Example11Property, value);
    }}

    public string? Example12
    {{
        get => (string?)GetValue(Example12Property);
        set => SetValue(Example12Property, value);
    }}

    public static int GetExampleAttached1({BaseClassName} target)
    {{
        return (int)target.GetValue(ExampleAttached1Property);
    }}

    public static void SetExampleAttached1({BaseClassName} target, int value)
    {{
        target.SetValue(ExampleAttached1Property, value);
    }}

    public static int GetExampleAttached2({BaseClassName} target)
    {{
        return (int)target.GetValue(ExampleAttached2Property);
    }}

    public static void SetExampleAttached2({BaseClassName} target, int value)
    {{
        target.SetValue(ExampleAttached2Property, value);
    }}

    public static string? GetExampleAttached3({BaseClassName} target)
    {{
        return (string?)target.GetValue(ExampleAttached3Property);
    }}

    public static void SetExampleAttached3({BaseClassName} target, string? value)
    {{
        target.SetValue(ExampleAttached3Property, value);
    }}

    public static string? GetExampleAttached4({BaseClassName} target)
    {{
        return (string?)target.GetValue(ExampleAttached4Property);
    }}

    public static void SetExampleAttached4({BaseClassName} target, string? value)
    {{
        target.SetValue(ExampleAttached4Property, value);
    }}

    public static string? GetExampleAttached5({BaseClassName} target)
    {{
        return (string?)target.GetValue(ExampleAttached5Property);
    }}

    public static void SetExampleAttached5({BaseClassName} target, string? value)
    {{
        target.SetValue(ExampleAttached5Property, value);
    }}

    public static string? GetExampleAttached6({BaseClassName} target)
    {{
        return (string?)target.GetValue(ExampleAttached6Property);
    }}

    public static void SetExampleAttached6({BaseClassName} target, string? value)
    {{
        target.SetValue(ExampleAttached6Property, value);
    }}

    public static int GetExampleAttached7({BaseClassName} target)
    {{
        return (int)target.GetValue(ExampleAttached7Property);
    }}

    public static void SetExampleAttached7({BaseClassName} target, int value)
    {{
        target.SetValue(ExampleAttached7Property, value);
    }}

    public static int GetExampleAttached8({BaseClassName} target)
    {{
        return (int)target.GetValue(ExampleAttached8Property);
    }}

    public static void SetExampleAttached8({BaseClassName} target, int value)
    {{
        target.SetValue(ExampleAttached8Property, value);
    }}

    public static string? GetExampleAttached9({BaseClassName} target)
    {{
        return (string?)target.GetValue(ExampleAttached9Property);
    }}

    public static void SetExampleAttached9({BaseClassName} target, string? value)
    {{
        target.SetValue(ExampleAttached9Property, value);
    }}

    public static string? GetExampleAttached10({BaseClassName} target)
    {{
        return (string?)target.GetValue(ExampleAttached10Property);
    }}

    public static void SetExampleAttached10({BaseClassName} target, string? value)
    {{
        target.SetValue(ExampleAttached10Property, value);
    }}

    public static string? GetExampleAttached11({BaseClassName} target)
    {{
        return (string?)target.GetValue(ExampleAttached11Property);
    }}

    public static void SetExampleAttached11({BaseClassName} target, string? value)
    {{
        target.SetValue(ExampleAttached11Property, value);
    }}

    public static string? GetExampleAttached12({BaseClassName} target)
    {{
        return (string?)target.GetValue(ExampleAttached12Property);
    }}

    public static void SetExampleAttached12({BaseClassName} target, string? value)
    {{
        target.SetValue(ExampleAttached12Property, value);
    }}

    static ExampleClass()
    {{
        Example1Property = {DependencyPropertyName}.Register(
            ""Example1"",
            typeof(int),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata());
        
        Example2Property = {DependencyPropertyName}.Register(
            ""Example2"",
            typeof(int),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(PropertyChangedCallback));
        
        Example3Property = {DependencyPropertyName}.Register(
            ""Example3"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValue));
        
        Example4Property = {DependencyPropertyName}.Register(
            ""Example4"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(default, (FrameworkPropertyMetadataOptions)256));
        
        Example5Property = {DependencyPropertyName}.Register(
            ""Example5"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValue, PropertyChangedCallback));
        
        Example6Property = {DependencyPropertyName}.Register(
            ""Example6"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValue, (FrameworkPropertyMetadataOptions)256, PropertyChangedCallback));
        
        Example7Property = {DependencyPropertyName}.Register(
            ""Example7"",
            typeof(int),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(default, CoerceValueCallback));
        
        Example8Property = {DependencyPropertyName}.Register(
            ""Example8"",
            typeof(int),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(PropertyChangedCallback, CoerceValueCallback));
        
        Example9Property = {DependencyPropertyName}.Register(
            ""Example9"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValue, default, CoerceValueCallback));
        
        Example10Property = {DependencyPropertyName}.Register(
            ""Example10"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValue, PropertyChangedCallback, CoerceValueCallback));
        
        Example11Property = {DependencyPropertyName}.Register(
            ""Example11"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValue, (FrameworkPropertyMetadataOptions)256, default, CoerceValueCallback));
        
        Example12Property = {DependencyPropertyName}.Register(
            ""Example12"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValue, (FrameworkPropertyMetadataOptions)256, PropertyChangedCallback, CoerceValueCallback));
        
        ExampleAttached1Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached1"",
            typeof(int),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata());
        
        ExampleAttached2Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached2"",
            typeof(int),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(PropertyChangedCallbackAttached));
        
        ExampleAttached3Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached3"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValueAttached));
        
        ExampleAttached4Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached4"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(default, (FrameworkPropertyMetadataOptions)256));
        
        ExampleAttached5Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached5"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValueAttached, PropertyChangedCallbackAttached));
        
        ExampleAttached6Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached6"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValueAttached, (FrameworkPropertyMetadataOptions)256, PropertyChangedCallbackAttached));
        
        ExampleAttached7Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached7"",
            typeof(int),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(default, CoerceValueCallbackAttached));
        
        ExampleAttached8Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached8"",
            typeof(int),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(PropertyChangedCallbackAttached, CoerceValueCallbackAttached));
        
        ExampleAttached9Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached9"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValueAttached, default, CoerceValueCallbackAttached));
        
        ExampleAttached10Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached10"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValueAttached, PropertyChangedCallbackAttached, CoerceValueCallbackAttached));
        
        ExampleAttached11Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached11"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValueAttached, (FrameworkPropertyMetadataOptions)256, default, CoerceValueCallbackAttached));
        
        ExampleAttached12Property = {DependencyPropertyName}.RegisterAttached(
            ""ExampleAttached12"",
            typeof(string),
            typeof(ExampleClass),
            new FrameworkPropertyMetadata(DefaultValueAttached, (FrameworkPropertyMetadataOptions)256, PropertyChangedCallbackAttached, CoerceValueCallbackAttached));
        
    }}
}}";

		TestSourceCodeTemplate(sourceCode, expectedSourceCode);
	}
}
