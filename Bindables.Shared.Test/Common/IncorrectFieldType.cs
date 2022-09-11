using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	[Test]
	public void IncorrectFieldType()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly int Example;
}";

		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.IncorrectFieldType);
	}

	[Test]
	public void IncorrectReadOnlyFieldType()
	{
		const string sourceCodeTemplate = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly int Example;
}";
		TestSourceCodeTemplate(sourceCodeTemplate, Diagnostics.IncorrectFieldType);
	}
}