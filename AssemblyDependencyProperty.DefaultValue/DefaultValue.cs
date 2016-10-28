using System.Windows;
using Depattach;

// ReSharper disable CheckNamespace
// ReSharper disable MemberInitializerValueIgnored

public class DefaultValue : DependencyObject
{
	public int MyProperty
	{
		get { return (int)GetValue(MyPropertyProperty); }
		set { SetValue(MyPropertyProperty, value); }
	}
	public static readonly DependencyProperty MyPropertyProperty = DependencyProperty.Register(
		nameof(MyProperty),
		typeof(int),
		typeof(DefaultValue),
		new FrameworkPropertyMetadata(1));

	[DependencyProperty]
	public string Reference { get; set; } = "Default";

	public string ReferenceNotDependencyProperty { get; set; } = nameof(ReferenceNotDependencyProperty);

	[DependencyProperty]
	public int Value { get; set; } = 1;

	public int ValueNotDependencyProperty { get; set; } = 2;
}