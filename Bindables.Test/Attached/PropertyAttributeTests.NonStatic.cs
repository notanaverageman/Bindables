using System;
using Bindables.Fody;
using FluentAssertions;
using NUnit.Framework;

namespace Bindables.Test.Attached
{
	[TestFixture]
	public class PropertyAttributeTestsNonStatic
	{
		private const string Code = @"
using System.Windows;
using Bindables;

public class PropertyAttributeNonAuto
{
	[AttachedProperty]
	public int NonStatic { get; set; }
}";

		[Test]
		public void ValidateAttributeOnNonStaticPropertyThrowsExecption()
		{
			Action action = () => Weaver.Weave(Code);
			action.Should().Throw<WeavingException>();
		}
	}
}