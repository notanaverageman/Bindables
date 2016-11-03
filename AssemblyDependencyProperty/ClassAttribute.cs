using System.Windows;
using Depattach;

// ReSharper disable CheckNamespace
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnassignedGetOnlyAutoProperty

[DependencyProperty]
public class ClassAttribute : DependencyObject
{
	private int _nonAuto;

	public int NonAuto
	{
		get { return _nonAuto; }
		set { _nonAuto = value; }
	}

    public int ReadOnly { get; }

    [ExcludeDependencyProperty]
    public int Excluded { get; set; }
	
	public string Reference { get; set; }
	public int Value { get; set; }
}