
// ReSharper disable CheckNamespace
// ReSharper disable ConvertToAutoProperty

using Depattach;
using System.Windows;

public class PropertyAttributeInvalidCallbackMethod : DependencyObject
{
	[DependencyProperty(OnPropertyChanged = nameof(PropertyChanged))]
	public int WithCallback { get; set; }

	private void PropertyChanged()
	{

	}
}