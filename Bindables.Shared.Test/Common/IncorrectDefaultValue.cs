using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void IncorrectDefaultValueFieldType()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
    private static string ExamplePropertyDefaultValue;

	[AttributeName(typeof(int), DefaultValueField = nameof(ExamplePropertyDefaultValue))]
	public static readonly PropertyType ExampleProperty;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.IncorrectDefaultValueFieldDefinition);
	}

	[Test]
	public void NonStaticDefaultValueField()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
    private int ExamplePropertyDefaultValue;

	[AttributeName(typeof(int), DefaultValueField = nameof(ExamplePropertyDefaultValue))]
	public static readonly PropertyType ExampleProperty;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.IncorrectDefaultValueFieldDefinition);
	}
}