using System.Windows;
using Depattach;

// ReSharper disable CheckNamespace
// ReSharper disable UnassignedGetOnlyAutoProperty

public class PropertyAttributeReadOnly : DependencyObject
{
	[DependencyProperty]
	public string Property { get; }
}