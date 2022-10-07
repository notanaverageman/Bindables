using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void ClassShouldBePartial()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public class InvalidClass : {BaseClassName}
{{
	[{DependencyPropertyAttributeName}(typeof(int))]
	public static readonly {DependencyPropertyName} ExampleProperty;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.ClassShouldBePartial);
	}
}