using System;
using System.Windows;

namespace Depattach
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
	public class DependencyPropertyAttribute : Attribute
	{
		public FrameworkPropertyMetadataOptions Options { get; set; }
		public string OnPropertyChanged { get; set; }
	}
}