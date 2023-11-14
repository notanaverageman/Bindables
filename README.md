
# Roslyn source generator to create dependency and attached properties for WPF, Xamarin.Forms, and .NET MAUI

[![NuGet Bindables.Wpf](https://img.shields.io/nuget/v/Bindables.Wpf.svg?label=Bindables.Wpf)](https://www.nuget.org/packages/Bindables.Wpf/)
[![NuGet Bindables.Forms](https://img.shields.io/nuget/v/Bindables.Forms.svg?label=Bindables.Forms)](https://www.nuget.org/packages/Bindables.Forms/)
[![NuGet Bindables.Forms](https://img.shields.io/nuget/v/Bindables.Maui.svg?label=Bindables.Maui)](https://www.nuget.org/packages/Bindables.Maui/)
[![AppVeyor](https://img.shields.io/appveyor/ci/notanaverageman/bindables.svg)](https://ci.appveyor.com/project/notanaverageman/bindables)

## Usage

Define a field with type __Dependency Property Type__ below and give it a name that ends with __Property Suffix__. Then, add __Attribute Type__ attribute to the field and pass the type of the property you want to create. Possible values are:

|Project Type |Dependency Property Type|Access Type|Property Suffix|Field Type             |Attribute Type                             |
|-------------|------------------------|-----------|---------------|-----------------------|-------------------------------------------|
|WPF          |Dependency Property     |Read/Write |`Property`     |`DependencyProperty`   |`Bindables.Wpf.DependencyPropertyAttribute`|
|WPF          |Dependency Property     |Read Only  |`PropertyKey`  |`DependencyPropertyKey`|`Bindables.Wpf.DependencyPropertyAttribute`|
|WPF          |Attached Property       |Read/Write |`Property`     |`DependencyProperty`   |`Bindables.Wpf.AttachedPropertyAttribute`  |
|WPF          |Attached Property       |Read Only  |`PropertyKey`  |`DependencyPropertyKey`|`Bindables.Wpf.AttachedPropertyAttribute`  |
|Xamarin.Forms|Bindable Property       |Read/Write |`Property`     |`BindableProperty`     |`Bindables.Forms.BindablePropertyAttribute`|
|Xamarin.Forms|Bindable Property       |Read Only  |`PropertyKey`  |`BindablePropertyKey`  |`Bindables.Forms.BindablePropertyAttribute`|
|Xamarin.Forms|Attached Property       |Read/Write |`Property`     |`BindableProperty`     |`Bindables.Forms.AttachedPropertyAttribute`|
|Xamarin.Forms|Attached Property       |Read Only  |`PropertyKey`  |`BindablePropertyKey`  |`Bindables.Forms.AttachedPropertyAttribute`|
|.NET MAUI    |Bindable Property       |Read/Write |`Property`     |`BindableProperty`     |`Bindables.Maui.BindablePropertyAttribute` |
|.NET MAUI    |Bindable Property       |Read Only  |`PropertyKey`  |`BindablePropertyKey`  |`Bindables.Maui.BindablePropertyAttribute` |
|.NET MAUI    |Attached Property       |Read/Write |`Property`     |`BindableProperty`     |`Bindables.Maui.AttachedPropertyAttribute` |
|.NET MAUI    |Attached Property       |Read Only  |`PropertyKey`  |`BindablePropertyKey`  |`Bindables.Maui.AttachedPropertyAttribute` |

## Options

You can pass following options:

|Option                       | Description                                                                       |
|-----------------------------|-----------------------------------------------------------------------------------|
|`OnPropertyChanged`          | Name of the method that will be called when the property is changed.              |
|`OnCoerceValue` (WPF)        | Name of the method that will be called when the property is re-evaluated/coerced. |
|`DefaultValueField`          | Name of the static field that will provide the default value for the property.    |
|`Options` (WPF)              | Pass `System.Windows.FrameworkPropertyMetadataOptions` to the dependency property.|
|`BindingMode` (Xamarin.Forms)| Pass `Xamarin.Forms.BindingMode` to the dependency property.                      |
|`BindingMode` (.NET MAUI)    | Pass `Microsoft.Maui.Controls.BindingMode` to the dependency property.            |

Signature of `OnPropertyChanged` method should be:
|Project Type             |Signature                                                                              |
|-------------------------|---------------------------------------------------------------------------------------|
|WPF                      |`static void MethodName(DependencyObject obj, DependencyPropertyChangedEventArgs args)`|
|Xamarin.Forms & .NET MAUI|`static void MethodName(BindableObject obj, object oldValue, object newValue)`         |

Signature of `OnCoerceValue` method should be:
|Project Type             |Signature                                                     |
|-------------------------|--------------------------------------------------------------|
|WPF                      |`static object MethodName(DependencyObject obj, object value)`|
|Xamarin.Forms & .NET MAUI|`static object MethodName(BindableObject obj, object value)`  |

## Requirements

- Your class should be `partial`.
- If you create a dependency property (WPF) or bindable property (Xamarin.Forms & .NET MAUI), your class should inherit from `System.Windows.DependencyObject` or `Xamarin.Forms.BindableObject` or `Microsoft.Maui.Controls.BindableObject` according to the project type. Attached properties don't have this requirement.
- Bindables creates the static constructor for the class to initialize the dependency properties. If you have custom static constructor for a type, you can't use Bindables on it.

## Example (WPF)

### Your Code

```c#
using System.Windows;
using Bindables.Wpf;

public partial class YourClass : DependencyObject
{
    private static readonly string DefaultValue = "Test";

    [DependencyProperty(typeof(string))]
    public static readonly DependencyProperty RegularProperty;

    // You can use any visibility modifier.
    [DependencyProperty(typeof(string))]
    private static readonly DependencyPropertyKey ReadOnlyPropertyKey;

    [DependencyProperty(typeof(string), OnPropertyChanged = nameof(PropertyChangedCallback), DefaultValueField = nameof(DefaultValue), Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
    public static readonly DependencyProperty CustomizedProperty;

    private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
    }
}
```

### Generated Code

```c#
// Generated by Bindables
using System.Windows;

#nullable enable

public partial class YourClass
{
    [global::System.CodeDom.Compiler.GeneratedCode("Bindables.Wpf.WpfPropertyGenerator", "1.4.0")]
    public string? Regular
    {
        get => (string?)GetValue(RegularProperty);
        set => SetValue(RegularProperty, value);
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Bindables.Wpf.WpfPropertyGenerator", "1.4.0")]
    public static readonly DependencyProperty ReadOnlyProperty;

    [global::System.CodeDom.Compiler.GeneratedCode("Bindables.Wpf.WpfPropertyGenerator", "1.4.0")]
    public string? ReadOnly
    {
        get => (string?)GetValue(ReadOnlyProperty);
        private set => SetValue(ReadOnlyPropertyKey, value);
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Bindables.Wpf.WpfPropertyGenerator", "1.4.0")]
    public string? Customized
    {
        get => (string?)GetValue(CustomizedProperty);
        set => SetValue(CustomizedProperty, value);
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Bindables.Wpf.WpfPropertyGenerator", "1.4.0")]
    static YourClass()
    {
        RegularProperty = DependencyProperty.Register(
            nameof(Regular),
            typeof(string),
            typeof(YourClass),
            new FrameworkPropertyMetadata());

        ReadOnlyPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ReadOnly),
            typeof(string),
            typeof(YourClass),
            new FrameworkPropertyMetadata());

        ReadOnlyProperty = ReadOnlyPropertyKey.DependencyProperty;

        CustomizedProperty = DependencyProperty.Register(
            nameof(Customized),
            typeof(string),
            typeof(YourClass),
            new FrameworkPropertyMetadata(DefaultValue, (FrameworkPropertyMetadataOptions)256, PropertyChangedCallback));
    }
}
```
