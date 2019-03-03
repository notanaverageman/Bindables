using System;
using Bindables.Fody;
using FluentAssertions;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[TestFixture]
	public class PropertyAttributeTestsInvalidCallbackMethod
	{
		private const string PropertyChangedCode = @"
using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod : DependencyObject
{
	[DependencyProperty(OnPropertyChanged = nameof(PropertyChanged))]
	public int WithCallback { get; set; }

	private void PropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{

	}
}";

		private const string CoerceValueCode = @"
using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod : DependencyObject
{
	[DependencyProperty(OnPropertyChanged = nameof(PropertyChanged), OnCoerceValue = nameof(CoerceValue))]
	public int WithCallback { get; set; }

	private static void PropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{

	}

	private static object CoerceValue(DependencyObject dependencyObject, string value)
	{
		return value;
	}
}";

		private const string MissingPropertyChangedCode = @"
using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod : DependencyObject
{
	[DependencyProperty(OnCoerceValue = nameof(CoerceValue))]
	public int WithCallback { get; set; }

	private static object CoerceValue(DependencyObject dependencyObject, object value)
	{
		return value;
	}
}";

		[Test]
		public void ValidateAttributeWithInvalidPropertyChangedCallbackMethodThrowsExecption()
		{
			Action action = () => Weaver.Weave(PropertyChangedCode);
			action.Should().Throw<WeavingException>();
		}

		[Test]
		public void ValidateAttributeWithInvalidCoerceValueCallbackMethodThrowsExecption()
		{
			Action action = () => Weaver.Weave(CoerceValueCode);
			action.Should().Throw<WeavingException>();
		}

		[Test]
		public void ValidateAttributeWithMissingPropertyChangedCallbackMethodThrowsExecption()
		{
			Action action = () => Weaver.Weave(MissingPropertyChangedCode);
			action.Should().Throw<WeavingException>();
		}
	}
}