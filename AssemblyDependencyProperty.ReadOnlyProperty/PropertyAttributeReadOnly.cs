using System.Windows;
using Bindables;

// ReSharper disable CheckNamespace
// ReSharper disable UnassignedGetOnlyAutoProperty

public class PropertyAttributeReadOnly : DependencyObject
{
	[DependencyProperty]
	public string Property { get; }
}