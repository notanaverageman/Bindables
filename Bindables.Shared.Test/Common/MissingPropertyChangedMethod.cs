using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void MissingPropertyChangedMethod()
	{
		string sourceCode = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
	[{DependencyPropertyAttributeName}(typeof(int), OnPropertyChanged = ""PropertyChangedCallback"")]
	public static readonly {DependencyPropertyName} ExampleProperty;
}}";

		TestSourceCodeTemplate(sourceCode, Diagnostics.MissingPropertyChangedMethod);
	}
}