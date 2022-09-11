using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void ClassShouldNotHaveStaticConstructor()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
    static InvalidClass()
    {
    }
    
	[AttributeName(typeof(int))]
	public static readonly PropertyType ExampleProperty;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.ClassShouldNotHaveStaticConstructor);
	}
}