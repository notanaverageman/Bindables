using System;
using System.Reflection;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[Ignore("Not working.")]
	[TestFixture]
	public class GenericClassTests
	{
		private Assembly _assembly;

		private const string Code = @"
using System.Windows;
using Bindables;

public class NonGeneric : Generic<string>
{
}

public class Generic<T>
{
	public static bool IsPropertyChangedCallbackCalled { get; set; }

    [DependencyProperty(OnPropertyChanged = nameof(OnPropertyChanged))]
    public object Test { get; set; }

	private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{
		IsPropertyChangedCallbackCalled = true;
	}
}";

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Code);
		}

		[Test]
		public void ValidateProperty()
		{
			Type type = _assembly.GetType("NonGeneric");
			Type baseType = type.BaseType;

			dynamic instance = Activator.CreateInstance(type);
			instance.Test = "Some value";

			bool property = (bool)baseType.GetProperty("IsPropertyChangedCallbackCalled").GetValue(null);

			Assert.AreEqual(true, property);
		}
	}
}