using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Test;

public static class MissingDefaultValueFieldExtensions
{
	private const string SourceCode = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int), DefaultValueField = ""ExamplePropertyDefaultValue"")]
	public static readonly PropertyType ExampleProperty;
}";

	public static void MissingDefaultValueField<T>(this TestBase testBase, string attributeName) where T : IIncrementalGenerator, new()
	{
		string sourceCode = SourceCode.ReplacePlaceholders(testBase, attributeName);

		TestResult result = testBase.Generate<T>(sourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.MissingDefaultValueField));

		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}
}