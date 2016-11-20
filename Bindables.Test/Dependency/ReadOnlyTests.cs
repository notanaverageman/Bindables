using System;
using System.Reflection;
using System.Windows;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[TestFixture]
	public class ReadOnlyTests
	{
		private Assembly _assembly;

		private const string Code = @"
using System.Windows;
using Bindables;

public class ReadOnly : DependencyObject
{
	[DependencyProperty(IsReadOnly = true)]
	public string ReadOnlyProperty { get; private set; }
}";

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Code);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyReadOnlyProperty()
		{
			Type type = _assembly.GetType("ReadOnly");

			DependencyPropertyKey readOnlyPropertyKey = (DependencyPropertyKey)type.GetField("ReadOnlyPropertyPropertyKey", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);
			
			Assert.IsNotNull(readOnlyPropertyKey);

			DependencyProperty readOnlyProperty = (DependencyProperty)type.GetField("ReadOnlyPropertyProperty").GetValue(null);
			MethodInfo setterMethod = type.GetMethod("set_ReadOnlyProperty", BindingFlags.NonPublic | BindingFlags.Instance);

			dynamic instance = Activator.CreateInstance(type);

			Assert.Throws<RuntimeBinderException>(() => instance.ReadOnlyProperty = "Test1");

			setterMethod.Invoke(instance, new object[] { "Test1" });

			Assert.AreEqual("Test1", instance.ReadOnlyProperty);
			Assert.AreEqual("Test1", instance.GetValue(readOnlyProperty));
		}
	}
}