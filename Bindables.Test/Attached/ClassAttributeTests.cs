using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;

namespace Bindables.Test.Attached
{
	[TestFixture]
	public class ClassAttributeTests
	{
		private Assembly _assembly;
		private DependencyObject _object;

		private const string Code = @"
using System.Windows;
using Bindables;

[AttachedProperty]
public class ClassAttribute
{
	private static int _nonAuto;

	public static int NonAuto
	{
		get { return _nonAuto; }
		set { _nonAuto = value; }
	}

    public static int ReadOnly { get; }

    [ExcludeAttachedProperty]
    public static int Excluded { get; set; }

    public int Instance { get; set; }
	
	public static string Reference { get; set; }
	public static int Value { get; set; }
}";

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Code);
			_object = new DependencyObject();
		}

		[Test]
		public void ValidateConversionToAttachedPropertyReferenceType()
		{
			Type type = _assembly.GetType("ClassAttribute");

			MethodInfo setter = type.GetMethod("SetReference", new[] { typeof(DependencyObject), typeof(string) });
			MethodInfo getter = type.GetMethod("GetReference", new[] { typeof(DependencyObject) });

			setter.Invoke(null, new[] { (object)_object, "Test" });

			string value = (string)getter.Invoke(null, new[] { (object)_object });
			
			Assert.AreEqual("Test", value);
		}

		[Test]
		public void ValidateConversionToAttachedPropertyValueType()
		{
			Type type = _assembly.GetType("ClassAttribute");

			MethodInfo setter = type.GetMethod("SetValue", new[] { typeof(DependencyObject), typeof(int) });
			MethodInfo getter = type.GetMethod("GetValue", new[] { typeof(DependencyObject) });

			setter.Invoke(null, new[] { (object)_object, 1 });

			int value = (int)getter.Invoke(null, new[] { (object)_object });

			Assert.AreEqual(1, value);
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

		[Test]
		public void ValidateInstancePropertiesAreNotTouched()
		{
			Type type = _assembly.GetType("ClassAttribute");
			FieldInfo fieldInfo = type.GetField("InstanceProperty");

			Assert.IsNull(fieldInfo);
		}
	}
}