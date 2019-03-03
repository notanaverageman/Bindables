using System;
using Bindables.Fody;
using FluentAssertions;
using NUnit.Framework;

namespace Bindables.Test.Attached
{
	[TestFixture]
	public class PropertyAttributeTestsReadOnly
	{
		private const string Code = @"
using System.Windows;
using Bindables;

public class PropertyAttributeReadOnly
{
	[AttachedProperty]
	public static string Property { get; }
}";

		[Test]
		public void ValidateAttributeOnNonAutoPropertyThrowsException()
		{
			Action action = () => Weaver.Weave(Code);
			action.Should().Throw<WeavingException>();
		}
	}
}