using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Test;

public static class IncorrectPropertyChangedMethodSignatureExtensions
{
	private const string SourceCode = @"
using PlatformNamespace;
using AttributeNamespace;

public partial class InvalidClass : BaseClassName
{
	[AttributeName(typeof(int), OnPropertyChanged = nameof(PropertyChangedCallback))]
	public static readonly PropertyType ExampleProperty;

    PropertyChangedCallbackMethodSignature
    {
    }
}";
	
	public static void IncorrectPropertyChangedMethodSignatureInvalidParameters<T>(
		this TestBase testBase,
		string attributeName)
		where T : IIncrementalGenerator, new()
	{
		Assert.Multiple(() =>
		{
			string[] validParameterTypes = testBase.PropertyChangedMethodParameterTypes;

			string[][] parameterTypesToTest =
			{
				Enumerable.Repeat("int", validParameterTypes.Length).ToArray(),
				validParameterTypes.Skip(1).ToArray(),
				validParameterTypes.Take(validParameterTypes.Length - 1).ToArray(),
				validParameterTypes.Take(validParameterTypes.Length - 1).Concat(new []{ "int" }).ToArray()
			};
			
			foreach (string[] parameterTypes in parameterTypesToTest)
			{
				string allParameters = string.Join(", ", parameterTypes.Select((parameter, i) => $"{parameter} arg{i}"));
				string methodSignature = $"private static void PropertyChangedCallback({allParameters})";

				string sourceCode = SourceCode
					.ReplacePlaceholders(testBase, attributeName)
					.Replace("PropertyChangedCallbackMethodSignature", methodSignature);

				Test<T>(testBase, sourceCode);
			}
		});
	}

	public static void IncorrectPropertyChangedMethodSignatureNonStatic<T>(
		this TestBase testBase,
		string attributeName)
		where T : IIncrementalGenerator, new()
	{
		string[] validParameterTypes = testBase.PropertyChangedMethodParameterTypes;

		string allParameters = string.Join(", ", validParameterTypes.Select((parameter, i) => $"{parameter} arg{i}"));
		string methodSignature = $"private void PropertyChangedCallback({allParameters})";

		string sourceCode = SourceCode
			.ReplacePlaceholders(testBase, attributeName)
			.Replace("PropertyChangedCallbackMethodSignature", methodSignature);

		Test<T>(testBase, sourceCode);
	}

	public static void IncorrectPropertyChangedMethodSignatureReturnType<T>(
		this TestBase testBase,
		string attributeName)
		where T : IIncrementalGenerator, new()
	{
		string[] validParameterTypes = testBase.PropertyChangedMethodParameterTypes;

		string allParameters = string.Join(", ", validParameterTypes.Select((parameter, i) => $"{parameter} arg{i}"));
		string methodSignature = $"private static int PropertyChangedCallback({allParameters})";

		string sourceCode = SourceCode
			.ReplacePlaceholders(testBase, attributeName)
			.Replace("PropertyChangedCallbackMethodSignature", methodSignature);

		Test<T>(testBase, sourceCode, diagnosticCount: 2);
	}

	private static void Test<T>(TestBase testBase, string sourceCode, int diagnosticCount = 1) where T : IIncrementalGenerator, new()
	{
		TestResult result = testBase.Generate<T>(sourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.IncorrectPropertyChangedMethodSignature));

		Assert.That(result.Diagnostics.Count, Is.EqualTo(diagnosticCount));
		Assert.That(error, Is.Not.Null);
	}
}