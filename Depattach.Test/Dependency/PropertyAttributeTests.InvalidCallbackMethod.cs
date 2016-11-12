using System;
using Depattach.Fody;
using FluentAssertions;
using Mono.Cecil;
using NUnit.Framework;

namespace Depattach.Test.Dependency
{
	[TestFixture]
	public class PropertyAttributeTestsInvalidCallbackMethod
	{
		[Test]
		public void ValidateAttributeWithInvalidCallbackMethodThrowsExecption()
		{
			ModuleDefinition module = ModuleDefinition.ReadModule("AssemblyDependencyProperty.PropertyChangedCallback.dll");

			ModuleWeaver weavingTask = new ModuleWeaver
			{
				ModuleDefinition = module
			};

			Action action = () => weavingTask.Execute();
			action.ShouldThrow<InvalidOperationException>();
		}
	}
}