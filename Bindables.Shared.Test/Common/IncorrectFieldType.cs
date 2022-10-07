using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void IncorrectFieldType()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
	[{DependencyPropertyAttributeName}(typeof(int))]
	public static readonly int Example;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectFieldType);
	}

	[Test]
	public void IncorrectReadOnlyFieldType()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
	[{DependencyPropertyAttributeName}(typeof(int))]
	public static readonly int Example;
}}";
		TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectFieldType);
	}
}