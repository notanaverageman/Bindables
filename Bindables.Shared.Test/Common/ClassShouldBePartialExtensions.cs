using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Test;

public static class ClassShouldBePartialExtensions
{
	private const string SourceCode = @"
using PlatformNamespace;
using AttributeNamespace;

public class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int))]
	public static readonly PropertyType ExampleProperty;
}";
	
	public static void ClassShouldBePartial<T>(this TestBase testBase, string attributeName) where T : IIncrementalGenerator, new()
	{
		string sourceCode = SourceCode.ReplacePlaceholders(testBase, attributeName);

		TestResult result = testBase.Generate<T>(sourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.ClassShouldBePartial));
		
		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}
}