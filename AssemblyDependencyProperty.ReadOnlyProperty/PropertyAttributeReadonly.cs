using System.Windows;
using Depattach;

// ReSharper disable CheckNamespace
// ReSharper disable UnassignedGetOnlyAutoProperty

public class PropertyAttributeReadonly : DependencyObject
{
	[DependencyProperty]
	public string ReadOnly { get; }
}