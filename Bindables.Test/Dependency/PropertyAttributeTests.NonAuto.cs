using System;
using Bindables.Fody;
using FluentAssertions;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[TestFixture]
	public class PropertyAttributeTestsNonAuto
	{
		private const string Code = @"
using System.Windows;
using Bindables;

public class PropertyAttributeNonAuto : DependencyObject
{
	private int _nonAuto;

	[DependencyProperty]
	public int NonAuto
	{
		get { return _nonAuto; }
		set { _nonAuto = value; }
	}
}";

		[Test]
		public void ValidateAttributeOnNonAutoPropertyThrowsException()
		{
			Action action = () => Weaver.Weave(Code);
			action.ShouldThrow<WeavingException>();
		}
	}
}