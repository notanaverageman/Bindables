using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[TestFixture]
	public class PropertyAttributeTests
	{
		private Assembly _assembly;

		private const string Code = @"
using System.Windows;
using Bindables;

public class PropertyAttribute : DependencyObject
{
	public static bool IsPropertyChangedCallbackCalled { get; set; }
	public static bool IsCoerceValueCallbackCalled { get; set; }

	[DependencyProperty]
	public string Reference { get; set; }

	[DependencyProperty]
	public int Value { get; set; }

	[DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public int WithOptions { get; set; }

	[DependencyProperty(OnPropertyChanged = nameof(OnPropertyChanged), OnCoerceValue = nameof(OnCoerceValue))]
	public int Callback { get; set; }

	private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{
		IsPropertyChangedCallbackCalled = true;
	}

	private static object OnCoerceValue(DependencyObject dependencyObject, object value)
	{
		IsCoerceValueCallbackCalled = true;
		return value;
	}
}";

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Code);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyReferenceType()
		{
			Type type = _assembly.GetType("PropertyAttribute");

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
			Type type = _assembly.GetType("PropertyAttribute");

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
		public void ValidateConversionToDependencyPropertyWithOptions()
		{
			Type type = _assembly.GetType("PropertyAttribute");

			DependencyProperty valueProperty = (DependencyProperty)type.GetField("WithOptionsProperty").GetValue(null);

			dynamic instance = Activator.CreateInstance(type);
			WithOptionsViewModel viewModel = new WithOptionsViewModel();

			Binding binding = new Binding(nameof(WithOptionsViewModel.Source))
			{
				Source = viewModel
			};
			BindingOperations.SetBinding(instance, valueProperty, binding);

			instance.WithOptions = 2;

			Assert.AreEqual(2, viewModel.Source);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyWithCallbacks()
		{
			Type type = _assembly.GetType("PropertyAttribute");

			dynamic instance = Activator.CreateInstance(type);
			instance.Callback = 2;

			PropertyInfo propertyChangedCallbackPropertyInfo = type.GetProperty("IsPropertyChangedCallbackCalled");
			bool isPropertyChangedCallbackCalled = (bool)propertyChangedCallbackPropertyInfo.GetValue(null);

			PropertyInfo coerceValueCallbackPropertyInfo = type.GetProperty("IsCoerceValueCallbackCalled");
			bool isCoerceValueCallbackCalled = (bool)coerceValueCallbackPropertyInfo.GetValue(null);
			
			Assert.AreEqual(true, isPropertyChangedCallbackCalled);
			Assert.AreEqual(true, isCoerceValueCallbackCalled);
		}

		private class WithOptionsViewModel
		{
			public int Source { get; set; }
		}
	}
}