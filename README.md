## This is an add-in for [Fody](https://github.com/Fody/Fody/) 

![Bindables Icon](https://raw.github.com/yusuf-gunaydin/Bindables/master/Icon.png)

_Bindables_ converts your auto properties into Wpf dependency properties.  
Additionally it allows you to set following options while registering the dependency property:

  - Specify a default value.
  - Specify `FrameworkPropertyMetadataOptions`.
  - Specify a `PropertyChangedCallback` method.
  - Register the dependency property as readonly.

[![AppVeyor](https://img.shields.io/appveyor/ci/yusuf-gunaydin/bindables.svg)](https://ci.appveyor.com/project/yusuf-gunaydin/bindables)
[![NuGet](https://img.shields.io/nuget/v/Bindables.Fody.svg)](https://www.nuget.org/packages/Bindables.Fody/)

## How To Use

You can apply the `DependencyPropertyAttribute` on the class.
In this case all the auto attributes in that class will be converted to dependency properties.

Non-auto properties and readonly properties will not be touched.  
You can exclude a property from conversion by applying `ExcludeDependencyPropertyAttribute` on the property.

Example:
```c#
[DependencyProperty]
public class YourClass : DependencyObject
{
    private int _nonAuto;
    public int NonAuto
    {
        get { return _nonAuto; }
        set { _nonAuto = value; }
    }

    public int ReadOnly { get; }

    [ExcludeDependencyProperty]
    public int Excluded { get; set; }

    public string Name { get; set; }
}
```

What gets compiled:
```c#
public class YourClass : DependencyObject
{
    private int _nonAuto;
    public int NonAuto
    {
        get { return _nonAuto; }
        set { _nonAuto = value; }
    }

    public int ReadOnly { get; }
    public int Excluded { get; set; }

    public string Name
    {
        get { return (string)GetValue(NameProperty); }
        set { SetValue(NameProperty, value); }
    }
    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
        nameof(Name),
        typeof(string),
        typeof(YourClass),
        new PropertyMetadata(default(string)));
}
```

---

You can specify the default value for the dependency property by initializing the auto property with that value.

```c#
[DependencyProperty]
public class YourClass : DependencyObject
{
    public string Name { get; set; } = "Default";
}
```

---

You can also apply the `DependencyPropertyAttribute` on individual properties.
If you prefer this way, you can set following options:

  - `FrameworkPropertyMetadataOptions` for the dependency property.
  - `PropertyChangedCallback` method.
    This setting expects that a method with signature `static void NameOfTheMethod(DependencyObject, DependencyPropertyChangedEventArgs)` exists in the class.
    ```c#
    public class PropertyAttribute : DependencyObject
    {
        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public int WithOptions { get; set; }
    
        [DependencyProperty(OnPropertyChanged = nameof(OnPropertyChanged))]
        public int WithCallback { get; set; }
    
        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
        }
    }
    ```
  - `IsReadOnly` to create a readonly dependency property.  
    Example:
    ```c#
    public class YourClass : DependencyObject
    {
        [DependencyProperty(IsReadOnly = true)]
        public string ReadOnly { get; protected set; }
    }
    ```
    What gets compiled:

    ```c#
    public class YourClass : DependencyObject
    {
        private static readonly DependencyPropertyKey ReadOnlyPropertyKey = DependencyProperty.RegisterReadOnly(
            "ReadOnlyProperty",
            typeof(int),
            typeof(YourClass),
            new PropertyMetadata(default(int)));
    
        public static readonly DependencyProperty ReadOnlyProperty = ReadOnlyPropertyKey.DependencyProperty;
    
        public int ReadOnly
        {
            get { return (int)GetValue(ReadOnlyProperty); }
            protected set { SetValue(ReadOnlyPropertyKey, value); }
        }
    }
    ```

## Icon

[Link](https://thenounproject.com/term/link/9266/) designed by [Dmitry Mirolyubov](https://thenounproject.com/dmitriy.mir/) from The Noun Project.