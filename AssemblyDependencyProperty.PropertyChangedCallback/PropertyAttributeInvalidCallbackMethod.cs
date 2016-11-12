
// ReSharper disable CheckNamespace
// ReSharper disable ConvertToAutoProperty

using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod : DependencyObject
{
	[DependencyProperty(OnPropertyChanged = nameof(PropertyChanged))]
	public int WithCallback { get; set; }

	private void PropertyChanged()
	{

	}
}