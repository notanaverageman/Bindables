using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Test;

public static class IncorrectFieldTypeExtensions
{
	private const string SimplePropertySourceCode = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly int Example;
}";

	private const string ReadOnlyPropertySourceCode = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly int Example;
}";

	public static void IncorrectFieldType<T>(this TestBase testBase, string attributeName) where T : IIncrementalGenerator, new()
	{
		string sourceCode = SimplePropertySourceCode.ReplacePlaceholders(testBase, attributeName);

		TestResult result = testBase.Generate<T>(sourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.IncorrectFieldType));

		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}

	public static void IncorrectReadOnlyFieldType<T>(this TestBase testBase, string attributeName) where T : IIncrementalGenerator, new()
	{
		string sourceCode = ReadOnlyPropertySourceCode.ReplacePlaceholders(testBase, attributeName);

		TestResult result = testBase.Generate<T>(sourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.IncorrectFieldType));

		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}
}