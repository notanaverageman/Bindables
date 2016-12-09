using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;

namespace Bindables.Test.Attached
{
	[TestFixture]
	public class DefaultValueTests
	{
		private Assembly _assembly;
		private DependencyObject _object;

		private const string Code = @"
using System.Windows;
using Bindables;

public class DefaultValue : DependencyObject
{
	[AttachedProperty]
	public static string Reference { get; set; } = ""Default"";

	[AttachedProperty]
	public static int Value { get; set; } = 1;
}";

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Code);
			_object = new DependencyObject();
		}

		[Test]
		public void ValidateDefaultValueReferenceType()
		{
			Type type = _assembly.GetType("DefaultValue");

			DependencyProperty referenceProperty = (DependencyProperty)type.GetField("ReferenceProperty").GetValue(null);
			
			Assert.AreEqual("Default", _object.GetValue(referenceProperty));
		}

		[Test]
		public void ValidateDefaultValueValueType()
		{
			Type type = _assembly.GetType("DefaultValue");

			DependencyProperty valueProperty = (DependencyProperty)type.GetField("ValueProperty").GetValue(null);
			
			Assert.AreEqual(1, _object.GetValue(valueProperty));
		}
	}
}