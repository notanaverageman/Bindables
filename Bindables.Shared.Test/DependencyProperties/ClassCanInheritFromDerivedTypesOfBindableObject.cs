using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class DependencyPropertyTests<T>
{
	[Test]
	public void ClassCanInheritFromDerivedTypesOfBindableObject()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class ExampleClass : DerivedFromBaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly PropertyType ExampleProperty;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, diagnosticDescriptor: null);
	}
}
