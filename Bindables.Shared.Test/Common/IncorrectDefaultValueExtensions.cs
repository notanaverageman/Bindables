using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Test;

public static class IncorrectDefaultValueExtensions
{
	private const string IncorrectTypeSourceCode = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
    private static string ExamplePropertyDefaultValue;

	[AttributeName(typeof(int), DefaultValueField = nameof(ExamplePropertyDefaultValue))]
	public static readonly PropertyType ExampleProperty;
}";

	private const string NonStaticSourceCode = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
    private int ExamplePropertyDefaultValue;

	[AttributeName(typeof(int), DefaultValueField = nameof(ExamplePropertyDefaultValue))]
	public static readonly PropertyType ExampleProperty;
}";

	public static void IncorrectDefaultValueFieldType<T>(this TestBase testBase, string attributeName) where T : IIncrementalGenerator, new()
	{
		string sourceCode = IncorrectTypeSourceCode.ReplacePlaceholders(testBase, attributeName);

		TestResult result = testBase.Generate<T>(sourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.IncorrectDefaultValueFieldDefinition));

		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}

	public static void NonStaticDefaultValueField<T>(this TestBase testBase, string attributeName) where T : IIncrementalGenerator, new()
	{
		string sourceCode = NonStaticSourceCode.ReplacePlaceholders(testBase, attributeName);

		TestResult result = testBase.Generate<T>(sourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.IncorrectDefaultValueFieldDefinition));

		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}
}