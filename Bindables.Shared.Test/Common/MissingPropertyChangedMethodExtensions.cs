using System.Linq;
using Bindables.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Test;

public static class MissingPropertyChangedMethodExtensions
{
	private const string SourceCode = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int), OnPropertyChanged = ""PropertyChangedCallback"")]
	public static readonly PropertyType ExampleProperty;
}";

	public static void MissingPropertyChangedMethod<T>(this TestBase testBase, string attributeName) where T : IIncrementalGenerator, new()
	{
		string sourceCode = SourceCode.ReplacePlaceholders(testBase, attributeName);

		TestResult result = testBase.Generate<T>(sourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.MissingPropertyChangedMethod));

		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}
}