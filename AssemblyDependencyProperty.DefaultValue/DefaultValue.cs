using System.Windows;
using Bindables;

// ReSharper disable CheckNamespace
// ReSharper disable MemberInitializerValueIgnored

public class DefaultValue : DependencyObject
{
	[DependencyProperty]
	public string Reference { get; set; } = "Default";

	[DependencyProperty]
	public int Value { get; set; } = 1;
}