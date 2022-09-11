using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void IncorrectFieldName()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly PropertyType Example;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.IncorrectFieldName);
	}

	[Test]
	public void IncorrectReadOnlyFieldName()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly KeyPropertyType Example;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.IncorrectReadOnlyFieldName);
	}
}