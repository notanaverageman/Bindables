using System.Windows;
using Bindables;

// ReSharper disable CheckNamespace
// ReSharper disable UnassignedGetOnlyAutoProperty

public class ReadOnly : DependencyObject
{
	[DependencyProperty]
	public string ReadOnlyProperty { get; private set; }
}