using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;

namespace Depattach.Test.Dependency
{
	[TestFixture]
	public class ClassAttributeTests
	{
		private Assembly _assembly;

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Weaver.DependencyProperty);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyReferenceType()
		{
			Type type = _assembly.GetType(nameof(ClassAttribute));

			DependencyProperty referenceProperty = (DependencyProperty)type.GetField($"{nameof(ClassAttribute.Reference)}Property").GetValue(null);

			dynamic instance = Activator.CreateInstance(type);

			instance.Reference = "Test1";
			Assert.AreEqual("Test1", instance.Reference);
			Assert.AreEqual("Test1", instance.GetValue(referenceProperty));

			instance.SetValue(referenceProperty, "Test2");
			Assert.AreEqual("Test2", instance.Reference);
			Assert.AreEqual("Test2", instance.GetValue(referenceProperty));
		}

		[Test]
		public void ValidateConversionToDependencyPropertyValueType()
		{
			Type type = _assembly.GetType(nameof(ClassAttribute));

			DependencyProperty valueProperty = (DependencyProperty)type.GetField($"{nameof(ClassAttribute.Value)}Property").GetValue(null);

			dynamic instance = Activator.CreateInstance(type);

			instance.Value = 1;
			Assert.AreEqual(1, instance.Value);
			Assert.AreEqual(1, instance.GetValue(valueProperty));

			instance.SetValue(valueProperty, 2);
			Assert.AreEqual(2, instance.Value);
			Assert.AreEqual(2, instance.GetValue(valueProperty));
		}

		[Test]
		public void ValidateNonAutoPropertiesAreNotTouched()
		{
			Type type = _assembly.GetType(nameof(ClassAttribute));
			FieldInfo fieldInfo = type.GetField($"{nameof(ClassAttribute.NonAuto)}Property");

			Assert.IsNull(fieldInfo);
		}

		[Test]
		public void ValidateReadOnlyPropertiesAreNotTouched()
		{
			Type type = _assembly.GetType(nameof(ClassAttribute));
			FieldInfo fieldInfo = type.GetField($"{nameof(ClassAttribute.ReadOnly)}Property");

			Assert.IsNull(fieldInfo);
		}

		[Test]
		public void ValidateExcludedPropertiesAreNotTouched()
		{
			Type type = _assembly.GetType(nameof(ClassAttribute));
			FieldInfo fieldInfo = type.GetField($"{nameof(ClassAttribute.Excluded)}Property");

			Assert.IsNull(fieldInfo);
		}
	}
}