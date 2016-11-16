using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[TestFixture]
	public class DefaultValueTests
	{
		private Assembly _assembly;

		private const string Code = @"
using System.Windows;
using Bindables;

public class DefaultValue : DependencyObject
{
	[DependencyProperty]
	public string Reference { get; set; } = ""Default"";

	[DependencyProperty]
	public int Value { get; set; } = 1;
}";

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Code);
		}

		[Test]
		public void ValidateDefaultValueReferenceType()
		{
			Type type = _assembly.GetType("DefaultValue");

			DependencyProperty referenceProperty = (DependencyProperty)type.GetField("ReferenceProperty").GetValue(null);

			dynamic instance = Activator.CreateInstance(type);
			
			Assert.AreEqual("Default", instance.GetValue(referenceProperty));
		}

		[Test]
		public void ValidateDefaultValueValueType()
		{
			Type type = _assembly.GetType("DefaultValue");

			DependencyProperty valueProperty = (DependencyProperty)type.GetField("ValueProperty").GetValue(null);

			dynamic instance = Activator.CreateInstance(type);
			
			Assert.AreEqual(1, instance.GetValue(valueProperty));
		}
	}
}