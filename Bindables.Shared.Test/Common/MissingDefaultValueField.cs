using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void MissingDefaultValueField()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int), DefaultValueField = ""ExamplePropertyDefaultValue"")]
	public static readonly PropertyType ExampleProperty;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.MissingDefaultValueField);
	}
}