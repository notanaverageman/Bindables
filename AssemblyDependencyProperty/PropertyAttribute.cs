using System.Windows;
using Bindables;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local

public class PropertyAttribute : DependencyObject
{
	public static bool IsCallbackCalled { get; set; }

	[DependencyProperty]
	public string Reference { get; set; }

	[DependencyProperty]
	public int Value { get; set; }

	[DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public int WithOptions { get; set; }

	[DependencyProperty(OnPropertyChanged = nameof(OnPropertyChanged))]
	public int PropertyChangedCallback { get; set; }

	private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{
		IsCallbackCalled = true;
	}
}