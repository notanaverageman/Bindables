using System;
using System.Reflection;
using System.Windows;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace Depattach.Test.Dependency
{
	[TestFixture]
	public class ReadOnlyTests
	{
		private Assembly _assembly;

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Weaver.DependencyPropertyReadOnly);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyReadOnlyProperty()
		{
			Type type = _assembly.GetType(nameof(ReadOnly));

			DependencyProperty readOnlyProperty = (DependencyProperty)type.GetField($"{nameof(ReadOnly.ReadOnlyProperty)}Property").GetValue(null);
			MethodInfo setterMethod = type.GetMethod($"set_{nameof(ReadOnly.ReadOnlyProperty)}", BindingFlags.NonPublic | BindingFlags.Instance);

			dynamic instance = Activator.CreateInstance(type);

			Assert.Throws<RuntimeBinderException>(() => instance.ReadOnlyProperty = "Test1");

			setterMethod.Invoke(instance, new object[] { "Test1" });

			Assert.AreEqual("Test1", instance.ReadOnlyProperty);
			Assert.AreEqual("Test1", instance.GetValue(readOnlyProperty));
		}
	}
}