using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using NUnit.Framework;

namespace Depattach.Test.Dependency
{
	[TestFixture]
	public class PropertyAttributeTests
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
			Type type = _assembly.GetType(nameof(PropertyAttribute));

			DependencyProperty referenceProperty = (DependencyProperty)type.GetField($"{nameof(PropertyAttribute.Reference)}Property").GetValue(null);

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
			Type type = _assembly.GetType(nameof(PropertyAttribute));

			DependencyProperty valueProperty = (DependencyProperty)type.GetField($"{nameof(PropertyAttribute.Value)}Property").GetValue(null);

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
			Type type = _assembly.GetType(nameof(PropertyAttribute));

			DependencyProperty valueProperty = (DependencyProperty)type.GetField($"{nameof(PropertyAttribute.WithOptions)}Property").GetValue(null);

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

		private class WithOptionsViewModel
		{
			public int Source { get; set; }
		}
	}
}