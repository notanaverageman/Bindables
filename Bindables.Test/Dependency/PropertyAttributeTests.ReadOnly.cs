using System;
using Bindables.Fody;
using FluentAssertions;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[TestFixture]
	public class PropertyAttributeTestsReadOnly
	{
		private const string Code = @"
using System.Windows;
using Bindables;

public class PropertyAttributeReadOnly : DependencyObject
{
	[DependencyProperty]
	public string Property { get; }
}";

		[Test]
		public void ValidateAttributeOnNonAutoPropertyThrowsException()
		{
			Action action = () => Weaver.Weave(Code);
			action.Should().Throw<WeavingException>();
		}
	}
}