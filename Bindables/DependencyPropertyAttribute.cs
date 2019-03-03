using System;
using System.Windows;

namespace Bindables
{
	/// <summary>
	/// Converts an auto property to a WPF dependency property.
	/// If applied to a class, converts all of the properties of that class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
	public class DependencyPropertyAttribute : Attribute
	{
		/// <summary>
		/// <see cref="FrameworkPropertyMetadataOptions"/> for the dependency property.
		/// </summary>
		public FrameworkPropertyMetadataOptions Options { get; set; }

		/// <summary>
		/// Name of the method that is given as PropertyChangedCallback while registering
		/// the dependency property.
		/// </summary>
		public string OnPropertyChanged { get; set; }
		
		/// <summary>
		/// Name of the method that is given as CoerceValueCallback while registering
		/// the dependency property.
		/// </summary>
		public string OnCoerceValue { get; set; }

		/// <summary>
		/// Set this to true to register the dependency property as readonly.
		/// </summary>
		public bool IsReadOnly { get; set; }
	}
}