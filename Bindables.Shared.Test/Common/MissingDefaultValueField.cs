using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void MissingDefaultValueField()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
	[{DependencyPropertyAttributeName}(typeof(int), DefaultValueField = ""ExamplePropertyDefaultValue"")]
	public static readonly {DependencyPropertyName} ExampleProperty;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.MissingDefaultValueField);
	}
}