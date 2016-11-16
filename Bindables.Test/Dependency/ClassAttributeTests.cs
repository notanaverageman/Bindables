using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[TestFixture]
	public class ClassAttributeTests
	{
		private Assembly _assembly;

		private const string Code = @"
using System.Windows;
using Bindables;

[DependencyProperty]
public class ClassAttribute : DependencyObject
{
	private int _nonAuto;

	public int NonAuto
	{
		get { return _nonAuto; }
		set { _nonAuto = value; }
	}

    public int ReadOnly { get; }

    [ExcludeDependencyProperty]
    public int Excluded { get; set; }
	
	public string Reference { get; set; }
	public int Value { get; set; }
}";

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Code);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyReferenceType()
		{
			Type type = _assembly.GetType("ClassAttribute");

			DependencyProperty referenceProperty = (DependencyProperty)type.GetField("ReferenceProperty").GetValue(null);

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
			Type type = _assembly.GetType("ClassAttribute");

			DependencyProperty valueProperty = (DependencyProperty)type.GetField("ValueProperty").GetValue(null);

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
			Type type = _assembly.GetType("ClassAttribute");
			FieldInfo fieldInfo = type.GetField("NonAutoProperty");

			Assert.IsNull(fieldInfo);
		}

		[Test]
		public void ValidateReadOnlyPropertiesAreNotTouched()
		{
			Type type = _assembly.GetType("ClassAttribute");
			FieldInfo fieldInfo = type.GetField("ReadOnlyProperty");

			Assert.IsNull(fieldInfo);
		}

		[Test]
		public void ValidateExcludedPropertiesAreNotTouched()
		{
			Type type = _assembly.GetType("ClassAttribute");
			FieldInfo fieldInfo = type.GetField("ExcludedProperty");

			Assert.IsNull(fieldInfo);
		}
	}
}