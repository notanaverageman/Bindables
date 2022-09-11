using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void ClassShouldBePartial()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly PropertyType ExampleProperty;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.ClassShouldBePartial);
	}
}