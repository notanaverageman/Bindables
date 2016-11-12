using System;
using Bindables.Fody;
using FluentAssertions;
using Mono.Cecil;
using NUnit.Framework;

namespace Bindables.Test.Dependency
{
	[TestFixture]
	public class PropertyAttributeTestsReadOnly
	{
		[Test]
		public void ValidateAttributeOnNonAutoPropertyThrowsExecption()
		{
			ModuleDefinition module = ModuleDefinition.ReadModule("AssemblyDependencyProperty.ReadOnlyProperty.dll");

			ModuleWeaver weavingTask = new ModuleWeaver
			{
				ModuleDefinition = module
			};

			Action action = () => weavingTask.Execute();
			action.ShouldThrow<InvalidOperationException>();
		}
	}
}