using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void IncorrectFieldName()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
	[{DependencyPropertyAttributeName}(typeof(int))]
	public static readonly {DependencyPropertyName} Example;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectFieldName);
	}

	[Test]
	public void IncorrectReadOnlyFieldName()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
	[{DependencyPropertyAttributeName}(typeof(int))]
	public static readonly {DependencyPropertyKeyName} Example;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectReadOnlyFieldName);
	}
}