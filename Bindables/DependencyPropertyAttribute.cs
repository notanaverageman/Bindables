using System;
using System.Windows;

namespace Bindables
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
	public class DependencyPropertyAttribute : Attribute
	{
		public FrameworkPropertyMetadataOptions Options { get; set; }
		public string OnPropertyChanged { get; set; }
	}
}