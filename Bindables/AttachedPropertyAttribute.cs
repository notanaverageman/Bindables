using System;
using System.Windows;

namespace Bindables
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
	public class AttachedPropertyAttribute : Attribute
	{
		public FrameworkPropertyMetadataOptions Options { get; set; }
		public string OnPropertyChanged { get; set; }
		public string OnCoerceValue { get; set; }
	}
}