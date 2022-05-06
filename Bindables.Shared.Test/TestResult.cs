using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Bindables.Test;

public class TestResult
{
	public List<SyntaxTree> SyntaxTrees { get; }
	public List<Diagnostic> Diagnostics { get; }

	public TestResult(List<SyntaxTree> syntaxTrees, List<Diagnostic> diagnostics)
	{
		SyntaxTrees = syntaxTrees;
		Diagnostics = diagnostics;
	}
}