using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class DependencyPropertyTests<T> : TestBase<T> where T : PropertyGeneratorBase, new()
{
	[Test]
	public void ClassShouldInheritFromDependencyObject()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass
{
	[AttributeName(typeof(int))]
	public static readonly PropertyType ExampleProperty;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, DoesNotInheritFromBaseClassDiagnosticDescriptor);
	}
}