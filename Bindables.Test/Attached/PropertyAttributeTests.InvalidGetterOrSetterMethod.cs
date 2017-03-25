using System;
using Bindables.Fody;
using FluentAssertions;
using NUnit.Framework;

namespace Bindables.Test.Attached
{
	[TestFixture]
	public class PropertyAttributeTestsInvalidGetterOrSetterMethod
	{
		private const string InvalidGetterReturnType = @"
using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod
{
	[AttachedProperty]
	public static int InvalidGetter { get; set; }

	private void GetInvalidGetter(DependencyObject dependencyObject)
	{
	}
}";

		private const string InvalidGetterBody = @"
using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod
{
	[AttachedProperty]
	public static int InvalidGetter { get; set; }

	private int GetInvalidGetter(DependencyObject dependencyObject)
	{
		return 0;
	}
}";

		private const string InvalidSetterReturnType = @"
using System;
using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod
{
	[AttachedProperty]
	public static int InvalidGetter { get; set; }

	private int SetInvalidGetter(DependencyObject dependencyObject, int value)
	{
		return 0;
	}
}";

		private const string InvalidSetterBody = @"
using System;
using System.Windows;
using Bindables;

public class PropertyAttributeInvalidCallbackMethod
{
	[AttachedProperty]
	public static int InvalidGetter { get; set; }

	private void SetInvalidGetter(DependencyObject dependencyObject, int value)
	{
		Console.WriteLine();
	}
}";

		[Test]
		public void ValidateAttributeWithInvalidGetterMethodReturnTypeThrowsExecption()
		{
			Action action = () => Weaver.Weave(InvalidGetterReturnType);
			action.ShouldThrow<WeavingException>();
		}

		[Test]
		public void ValidateAttributeWithInvalidGetterMethodBodyThrowsExecption()
		{
			Action action = () => Weaver.Weave(InvalidGetterBody);
			action.ShouldThrow<WeavingException>();
		}

		[Test]
		public void ValidateAttributeWithInvalidSetterMethodReturnTypeThrowsExecption()
		{
			Action action = () => Weaver.Weave(InvalidSetterReturnType);
			action.ShouldThrow<WeavingException>();
		}

		[Test]
		public void ValidateAttributeWithInvalidSetterMethodBodyThrowsExecption()
		{
			Action action = () => Weaver.Weave(InvalidSetterBody);
			action.ShouldThrow<WeavingException>();
		}
	}
}