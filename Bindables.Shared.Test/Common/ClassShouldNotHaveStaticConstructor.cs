using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void ClassShouldNotHaveStaticConstructor()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
    static InvalidClass()
    {{
    }}
    
	[{DependencyPropertyAttributeName}(typeof(int))]
	public static readonly {DependencyPropertyName} ExampleProperty;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.ClassShouldNotHaveStaticConstructor);
	}
}