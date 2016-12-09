using System;
using Bindables.Fody;
using FluentAssertions;
using NUnit.Framework;

namespace Bindables.Test.Attached
{
	[TestFixture]
	public class PropertyAttributeTestsNonAuto
	{
		private const string Code = @"
using System.Windows;
using Bindables;

public class PropertyAttributeNonAuto
{
	private static int _nonAuto;

	[AttachedProperty]
	public static int NonAuto
	{
		get { return _nonAuto; }
		set { _nonAuto = value; }
	}
}";

		[Test]
		public void ValidateAttributeOnNonAutoPropertyThrowsExecption()
		{
			Action action = () => Weaver.Weave(Code);
			action.ShouldThrow<WeavingException>();
		}
	}
}