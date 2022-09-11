using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void MissingPropertyChangedMethod()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int), OnPropertyChanged = ""PropertyChangedCallback"")]
	public static readonly PropertyType ExampleProperty;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.MissingPropertyChangedMethod);
	}
}