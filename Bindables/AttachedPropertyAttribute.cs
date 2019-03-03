using System;
using System.Windows;

namespace Bindables
{
	/// <summary>
	/// Converts an auto property to a WPF attached property.
	/// If applied to a class, converts all of the properties of that class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
	public class AttachedPropertyAttribute : Attribute
	{
		/// <summary>
		/// <see cref="FrameworkPropertyMetadataOptions"/> for the attached property.
		/// </summary>
		public FrameworkPropertyMetadataOptions Options { get; set; }

		/// <summary>
		/// Name of the method that is given as PropertyChangedCallback while registering
		/// the attached property.
		/// </summary>
		public string OnPropertyChanged { get; set; }
		
		/// <summary>
		/// Name of the method that is given as CoerceValueCallback while registering
		/// the attached property.
		/// </summary>
		public string OnCoerceValue { get; set; }
	}
}