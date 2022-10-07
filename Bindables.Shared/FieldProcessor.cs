using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Bindables;

public class FieldProcessor
{
	private readonly INamedTypeSymbol _attributeSymbol;
	private readonly Func<SourceProductionContext, INamedTypeSymbol, IFieldSymbol, INamedTypeSymbol, CheckResult> _checkFunction;
	private readonly Action<CodeBuilder, INamedTypeSymbol, IFieldSymbol, INamedTypeSymbol, List<string>> _processFunction;
	private readonly Action<CodeBuilder, INamedTypeSymbol, IFieldSymbol, INamedTypeSymbol, List<string>> _processReadOnlyFunction;

	public FieldProcessor(
		INamedTypeSymbol attributeSymbol,
		Func<SourceProductionContext, INamedTypeSymbol, IFieldSymbol, INamedTypeSymbol, CheckResult> checkFunction,
		Action<CodeBuilder, INamedTypeSymbol, IFieldSymbol, INamedTypeSymbol, List<string>> processFunction,
		Action<CodeBuilder, INamedTypeSymbol, IFieldSymbol, INamedTypeSymbol, List<string>> processReadOnlyFunction)
	{
		_attributeSymbol = attributeSymbol;
		_checkFunction = checkFunction;
		_processFunction = processFunction;
		_processReadOnlyFunction = processReadOnlyFunction;
	}

	public CheckResult Check(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		IFieldSymbol fieldSymbol)
	{
		return _checkFunction(context, classSymbol, fieldSymbol, _attributeSymbol);
	}

	public void Process(
		CodeBuilder builder,
		INamedTypeSymbol classSymbol,
		BindableField field,
		List<string> initializationLines)
	{
		if (field.PropertyType == PropertyType.ReadWrite)
		{
			_processFunction(builder, classSymbol, field.FieldSymbol, _attributeSymbol, initializationLines);
		}
		else
		{
			_processReadOnlyFunction(builder, classSymbol, field.FieldSymbol, _attributeSymbol, initializationLines);
		}
	}
}