using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Bindables.Test;

public abstract partial class TestBase<T>
{
	private readonly string IncorrectPropertyChangedMethodSignatureSourceCodeTemplate;

	public TestBase()
	{
		IncorrectPropertyChangedMethodSignatureSourceCodeTemplate = $@"
using {PlatformNamespace};
using {AttributeNamespace};

public partial class InvalidClass : {BaseClassName}
{{
	[{DependencyPropertyAttributeName}(typeof(int), OnPropertyChanged = nameof(PropertyChangedCallback))]
	public static readonly {DependencyPropertyName} ExampleProperty;

    PropertyChangedCallbackMethodSignature
    {{
		PropertyChangedCallbackMethodBody
    }}
}}";
	}

	[Test]
	public void IncorrectPropertyChangedMethodSignatureInvalidParameters()
	{
		Assert.Multiple(() =>
		{
			IReadOnlyList<string> validParameterTypes = PropertyChangedMethodParameterTypes;

			string[][] parameterTypesToTest =
			{
				Enumerable.Repeat("int", validParameterTypes.Count).ToArray(),
				validParameterTypes.Skip(1).ToArray(),
				validParameterTypes.Take(validParameterTypes.Count - 1).ToArray(),
				validParameterTypes.Take(validParameterTypes.Count - 1).Concat(new []{ "int" }).ToArray()
			};
			
			foreach (string[] parameterTypes in parameterTypesToTest)
			{
				string allParameters = string.Join(", ", parameterTypes.Select((parameter, i) => $"{parameter} arg{i}"));
				string methodSignature = $"private static void PropertyChangedCallback({allParameters})";

				string sourceCode = IncorrectPropertyChangedMethodSignatureSourceCodeTemplate
					.Replace("PropertyChangedCallbackMethodSignature", methodSignature)
					.Replace("PropertyChangedCallbackMethodBody", "");

				TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectPropertyChangedMethodSignature);
			}
		});
	}

	[Test]
	public void IncorrectPropertyChangedMethodSignatureNonStatic()
	{
		IReadOnlyList<string> validParameterTypes = PropertyChangedMethodParameterTypes;

		string allParameters = string.Join(", ", validParameterTypes.Select((parameter, i) => $"{parameter} arg{i}"));
		string methodSignature = $"private void PropertyChangedCallback({allParameters})";

		string sourceCode = IncorrectPropertyChangedMethodSignatureSourceCodeTemplate
			.Replace("PropertyChangedCallbackMethodSignature", methodSignature)
			.Replace("PropertyChangedCallbackMethodBody", "");

		TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectPropertyChangedMethodSignature);
	}

	[Test]
	public void IncorrectPropertyChangedMethodSignatureReturnType()
	{
		IReadOnlyList<string> validParameterTypes = PropertyChangedMethodParameterTypes;

		string allParameters = string.Join(", ", validParameterTypes.Select((parameter, i) => $"{parameter} arg{i}"));
		string methodSignature = $"private static int PropertyChangedCallback({allParameters})";

		string sourceCode = IncorrectPropertyChangedMethodSignatureSourceCodeTemplate
			.Replace("PropertyChangedCallbackMethodSignature", methodSignature)
			.Replace("PropertyChangedCallbackMethodBody", "return 0;");

		TestSourceCodeTemplate(sourceCode, Diagnostics.IncorrectPropertyChangedMethodSignature);
	}
}