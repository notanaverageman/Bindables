using System;
using Bindables.Fody;
using FluentAssertions;
using NUnit.Framework;

namespace Bindables.Test.Attached
{
	[TestFixture]
	public class PropertyAttributeTestsInvalidCallbackMethod
	{
		private const string Code = @"
using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod
{
	[AttachedProperty(OnPropertyChanged = nameof(PropertyChanged))]
	public static int WithCallback { get; set; }

	private void PropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{

	}
}";

		[Test]
		public void ValidateAttributeWithInvalidCallbackMethodThrowsExecption()
		{
			Action action = () => Weaver.Weave(Code);
			action.ShouldThrow<WeavingException>();
		}
	}
}