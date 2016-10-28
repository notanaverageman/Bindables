using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;

namespace Depattach.Test.Dependency
{
	[TestFixture]
	public class DefaultValueTests
	{
		private Assembly _assembly;

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Weaver.DependencyPropertyDefaultValue);
		}

		[Test]
		public void ValidateDefaultValueReferenceType()
		{
			Type type = _assembly.GetType(nameof(DefaultValue));

			DependencyProperty referenceProperty = (DependencyProperty)type.GetField($"{nameof(DefaultValue.Reference)}Property").GetValue(null);

			dynamic instance = Activator.CreateInstance(type);
			
			Assert.AreEqual("Default", instance.GetValue(referenceProperty));
		}

		[Test]
		public void ValidateDefaultValueValueType()
		{
			Type type = _assembly.GetType(nameof(DefaultValue));

			DependencyProperty valueProperty = (DependencyProperty)type.GetField($"{nameof(DefaultValue.Value)}Property").GetValue(null);

			dynamic instance = Activator.CreateInstance(type);
			
			Assert.AreEqual(1, instance.GetValue(valueProperty));
		}
	}
}