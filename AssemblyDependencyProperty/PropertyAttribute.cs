using System.Windows;
using Depattach;

// ReSharper disable CheckNamespace

public class PropertyAttribute : DependencyObject
{
	[DependencyProperty]
	public string Reference { get; set; }

	[DependencyProperty]
	public int Value { get; set; }
}