using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using NUnit.Framework;

namespace Bindables.Test.Attached
{
	[TestFixture]
	public class PropertyAttributeTests
	{
		private Assembly _assembly;
		private DependencyObject _object;

		private const string Code = @"
using System;
using System.Windows;
using Bindables;

public class PropertyAttribute
{
	public static bool IsPropertyChangedCallbackCalled { get; set; }
	public static bool IsCoerceValueCallbackCalled { get; set; }

	[AttachedProperty]
	public static string Reference { get; set; }

	[AttachedProperty]
	public static int Value { get; set; }

	[AttachedProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	public static int WithOptions { get; set; }

	[AttachedProperty(OnPropertyChanged = nameof(OnPropertyChanged), OnCoerceValue = nameof(OnCoerceValue))]
	public static int Callback { get; set; }

	private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{
		IsPropertyChangedCallbackCalled = true;
	}

	private static object OnCoerceValue(DependencyObject dependencyObject, object value)
	{
		IsCoerceValueCallbackCalled = true;
		return value;
	}

	[AttachedProperty]
	public static int WithEmptyGetterAndSetterMethods { get; set; }

	public static int GetWithEmptyGetterAndSetterMethods(DependencyObject o)
	{
		throw new WillBeImplementedByBindablesException();
	}
	
	public static void SetWithEmptyGetterAndSetterMethods(DependencyObject o, int value)
	{
	}

	[AttachedProperty]
	public static byte[] ByteArray { get; set; }

	public static byte[] GetByteArray(DependencyObject o)
	{
		throw new WillBeImplementedByBindablesException();
	}
	
	public static void SetByteArray(DependencyObject o, byte[] value)
	{
	}
}";

		[OneTimeSetUp]
		public void Setup()
		{
			_assembly = Weaver.Weave(Code);
			_object = new DependencyObject();
		}

		[Test]
		public void ValidateConversionToDependencyPropertyReferenceType()
		{
			Type type = _assembly.GetType("PropertyAttribute");

			MethodInfo setter = type.GetMethod("SetReference", new[] { typeof(DependencyObject), typeof(string) });
			MethodInfo getter = type.GetMethod("GetReference", new[] { typeof(DependencyObject) });

			setter.Invoke(null, new[] { (object)_object, "Test" });

			string value = (string)getter.Invoke(null, new[] { (object)_object });

			Assert.AreEqual("Test", value);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyValueType()
		{
			Type type = _assembly.GetType("PropertyAttribute");

			MethodInfo setter = type.GetMethod("SetValue", new[] { typeof(DependencyObject), typeof(int) });
			MethodInfo getter = type.GetMethod("GetValue", new[] { typeof(DependencyObject) });

			setter.Invoke(null, new[] { (object)_object, 1 });

			int value = (int)getter.Invoke(null, new[] { (object)_object });

			Assert.AreEqual(1, value);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyWithOptions()
		{
			Type type = _assembly.GetType("PropertyAttribute");

			DependencyProperty valueProperty = (DependencyProperty)type.GetField("WithOptionsProperty").GetValue(null);

			WithOptionsViewModel viewModel = new WithOptionsViewModel();

			Binding binding = new Binding(nameof(WithOptionsViewModel.Source))
			{
				Source = viewModel
			};
			BindingOperations.SetBinding(_object, valueProperty, binding);

			_object.SetValue(valueProperty, 1);

			Assert.AreEqual(1, viewModel.Source);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyWithPropertyChangedCallback()
		{
			Type type = _assembly.GetType("PropertyAttribute");

			DependencyProperty callbackProperty = (DependencyProperty)type.GetField("CallbackProperty").GetValue(null);
			_object.SetValue(callbackProperty, 1);
			
			PropertyInfo propertyChangedCallbackPropertyInfo = type.GetProperty("IsPropertyChangedCallbackCalled");
			bool isPropertyChangedCallbackCalled = (bool)propertyChangedCallbackPropertyInfo.GetValue(null);
			
			PropertyInfo coerceValueCallbackPropertyInfo = type.GetProperty("IsCoerceValueCallbackCalled");
			bool isCoerceValueCallbackCalled = (bool)coerceValueCallbackPropertyInfo.GetValue(null);

			Assert.AreEqual(true, isPropertyChangedCallbackCalled);
			Assert.AreEqual(true, isCoerceValueCallbackCalled);
		}

		[Test]
		public void ValidateConversionToDependencyPropertyWithEmptyGetterAndSetterMethods()
		{
			Type type = _assembly.GetType("PropertyAttribute");

			MethodInfo setter = type.GetMethod("SetWithEmptyGetterAndSetterMethods", new[] { typeof(DependencyObject), typeof(int) });
			MethodInfo getter = type.GetMethod("GetWithEmptyGetterAndSetterMethods", new[] { typeof(DependencyObject) });

			setter.Invoke(null, new[] { (object)_object, 2 });

			int value = (int)getter.Invoke(null, new[] { (object)_object });

			Assert.AreEqual(2, value);
		}

		private class WithOptionsViewModel
		{
			public int Source { get; set; }
		}
	}
}