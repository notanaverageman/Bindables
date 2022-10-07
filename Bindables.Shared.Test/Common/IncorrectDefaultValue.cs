using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void IncorrectDefaultValueFieldType()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
    private static string ExamplePropertyDefaultValue;

	[{DependencyPropertyAttributeName}(typeof(int), DefaultValueField = nameof(ExamplePropertyDefaultValue))]
	public static readonly {DependencyPropertyName} ExampleProperty;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectDefaultValueFieldDefinition);
	}

	[Test]
	public void NonStaticDefaultValueField()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
    private int ExamplePropertyDefaultValue;

	[{DependencyPropertyAttributeName}(typeof(int), DefaultValueField = nameof(ExamplePropertyDefaultValue))]
	public static readonly {DependencyPropertyName} ExampleProperty;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectDefaultValueFieldDefinition);
	}
}