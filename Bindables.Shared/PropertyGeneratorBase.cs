using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bindables;

public abstract class PropertyGeneratorBase : IIncrementalGenerator
{
	public abstract bool IsAttachedPropertyGenerator { get; }

	public abstract string AttributeName { get; }
	public abstract string AttributeNamespace { get; }
	public abstract string PlatformNamespace { get; }
	public abstract string AttributeSourceText { get; }
	public abstract string BaseClassName { get; }
	public abstract string DerivedFromBaseClassName { get; }
	public abstract string RegisterMethod { get; }
	public abstract string RegisterReadOnlyMethod { get; }
	public abstract string RegisterAttachedMethod { get; }
	public abstract string RegisterAttachedReadOnlyMethod { get; }

	public abstract IReadOnlyList<string> PropertyChangedMethodParameterTypes { get; }
	public abstract IReadOnlyList<string> CoerceValueMethodParameterTypes { get; }

	public IReadOnlyList<(string, string, DiagnosticDescriptor)> FieldTypeAndNameConditions { get; }

	public abstract string DependencyPropertyName { get; }
	public abstract string DependencyPropertyKeyName { get; }

	public abstract DiagnosticDescriptor DoesNotInheritFromBaseClassDiagnosticDescriptor { get; }

	public string AttributeTypeName => $"{AttributeNamespace}.{AttributeName}";

	public PropertyGeneratorBase()
	{
		// TODO: Use static interface
		FieldTypeAndNameConditions = new[]
		{
			($"{PlatformNamespace}.{DependencyPropertyName}", "Property", Diagnostics.IncorrectFieldName),
			($"{PlatformNamespace}.{DependencyPropertyKeyName}", "PropertyKey", Diagnostics.IncorrectReadOnlyFieldName),
		};
	}

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(i => i.AddSource($"{AttributeName}.g.cs", AttributeSourceText.Trim()));

		IncrementalValuesProvider<ClassDeclarationSyntax?> classDeclarations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (x, _) => IsSyntaxTargetForGeneration(x),
				transform: (x, _) => GetSemanticTargetForGeneration(x))
			.Where(static x => x is not null);

		context.RegisterSourceOutput(
			context.CompilationProvider.Combine(classDeclarations.Collect()),
			(compilation, source) => Execute(source.Left, source.Right, compilation));
	}

	private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
	{
		return node is FieldDeclarationSyntax { AttributeLists.Count: > 0 };
	}

	private ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
	{
		FieldDeclarationSyntax fieldSyntax = (FieldDeclarationSyntax)context.Node;

		foreach (AttributeListSyntax attributeList in fieldSyntax.AttributeLists)
		{
			foreach (AttributeSyntax attributeSyntax in attributeList.Attributes)
			{
				if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
				{
					continue;
				}

				INamedTypeSymbol attributeContainingType = attributeSymbol.ContainingType;
				string fullName = attributeContainingType.ToDisplayString();

				if (fullName == AttributeTypeName)
				{
					return fieldSyntax.Parent as ClassDeclarationSyntax;
				}
			}
		}

		return null;
	}

	private void Execute(
		Compilation compilation,
		ImmutableArray<ClassDeclarationSyntax?> classes,
		SourceProductionContext context)
	{
		INamedTypeSymbol? attributeSymbol = compilation.GetTypeByMetadataName(AttributeTypeName);

		if (attributeSymbol == null)
		{
			return;
		}

		foreach (ClassDeclarationSyntax? @class in classes.Distinct())
		{
			if (context.CancellationToken.IsCancellationRequested)
			{
				return;
			}

			if (@class == null)
			{
				continue;
			}

			SemanticModel semanticModel = compilation.GetSemanticModel(@class.SyntaxTree);

			if (ModelExtensions.GetDeclaredSymbol(semanticModel, @class) is not INamedTypeSymbol classSymbol)
			{
				continue;
			}

			ImmutableArray<ISymbol> classMembers = classSymbol.GetMembers();
			List<IFieldSymbol> fields = new();

			foreach (ISymbol member in classMembers)
			{
				if (member is not IFieldSymbol fieldSymbol)
				{
					continue;
				}

				if (!fieldSymbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == AttributeTypeName))
				{
					continue;
				}

				CheckResult checkResult = IsAttachedPropertyGenerator
					? CheckAttachedProperty(context, classSymbol, fieldSymbol, attributeSymbol)
					: CheckDependencyProperty(context, classSymbol, fieldSymbol, attributeSymbol);

				if (checkResult == CheckResult.Valid)
				{
					fields.Add(fieldSymbol);
				}
			}

			if (!fields.Any())
			{
				continue;
			}

			string? classSource = ProcessClass(classSymbol, fields, attributeSymbol);

			if (string.IsNullOrEmpty(classSource))
			{
				continue;
			}

			context.AddSource($"{classSymbol}_Bindables_{AttributeName}.g.cs", classSource!);
		}
	}
	
	private string? ProcessClass(INamedTypeSymbol classSymbol, List<IFieldSymbol> fields, ISymbol attributeSymbol)
	{
		if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
		{
			//TODO: Issue a diagnostic that it must be top level.
			return null;
		}

		INamespaceSymbol @namespace = classSymbol.ContainingNamespace;

		CodeBuilder builder = new();

		builder.AppendLine("// Generated by Bindables");
		builder.AppendLine($"using {PlatformNamespace};");
		builder.AppendLine();
		builder.AppendLine("#nullable enable");
		builder.AppendLine();

		if (!@namespace.IsGlobalNamespace)
		{
			builder.AppendLine($"namespace {@namespace}");
			builder.OpenScope();
		}

		builder.AppendLine($"public partial class {classSymbol.Name}");
		builder.OpenScope();

		List<string> initializationLines = new();

		foreach (IFieldSymbol field in fields)
		{
			ProcessField(builder, classSymbol, field, attributeSymbol, initializationLines);
		}

		builder.AppendLine($"static {classSymbol.Name}()");
		builder.OpenScope();

		foreach (string line in initializationLines)
		{
			builder.AppendLine(line);
		}

		builder.CloseScope();
		builder.CloseScope();

		if (!@namespace.IsGlobalNamespace)
		{
			builder.CloseScope();
		}

		return builder.ToString();
	}

	private void ProcessField(
		CodeBuilder builder,
		INamedTypeSymbol classSymbol,
		IFieldSymbol field,
		ISymbol attributeSymbol,
		List<string> initializationLines)
	{
		ITypeSymbol fieldType = field.Type;

		bool isDependencyProperty = fieldType.Name == DependencyPropertyName;
		bool isDependencyPropertyKey = fieldType.Name == DependencyPropertyKeyName;

		if (isDependencyProperty)
		{
			if (IsAttachedPropertyGenerator)
			{
				ProcessAttachedProperty(builder, classSymbol, field, attributeSymbol, initializationLines);
			}
			else
			{
				ProcessDependencyProperty(builder, classSymbol, field, attributeSymbol, initializationLines);
			}
		}
		else if (isDependencyPropertyKey)
		{
			if (IsAttachedPropertyGenerator)
			{
				ProcessAttachedPropertyKey(builder, classSymbol, field, attributeSymbol, initializationLines);
			}
			else
			{
				ProcessDependencyPropertyKey(builder, classSymbol, field, attributeSymbol, initializationLines);
			}
		}
		else
		{
			// TODO: Internal error.
		}
	}
	
	protected void ProcessDependencyProperty(
		CodeBuilder builder,
		INamedTypeSymbol classSymbol,
		IFieldSymbol field,
		ISymbol attributeSymbol,
		List<string> initializationLines)
	{
		AttributeData attributeData = field.GetAttributeData(attributeSymbol);
		INamedTypeSymbol? propertyType = attributeData.ConstructorArguments.SingleOrDefault().Value as INamedTypeSymbol;

		if (propertyType == null)
		{
			// TODO: Internal error.
			return;
		}

		string fieldName = field.Name;
		string fieldVisibility = SyntaxFacts.GetText(field.DeclaredAccessibility);
		string propertyName = fieldName.Substring(0, fieldName.Length - "Property".Length);
		string maybeNullPropertyTypeName = propertyType.ToDisplayString(NullableFlowState.MaybeNull);
		string propertyTypeName = propertyType.ToDisplayString();

		AppendPropertyWithGetterSetter(
			builder,
			maybeNullPropertyTypeName,
			propertyName,
			fieldVisibility,
			isReadOnly: false);

		AppendCreatePropertyExpression(
			RegisterMethod,
			initializationLines,
			attributeData,
			classSymbol,
			propertyTypeName,
			propertyName,
			fieldName);
	}

	protected void ProcessDependencyPropertyKey(
		CodeBuilder builder,
		INamedTypeSymbol classSymbol,
		IFieldSymbol field,
		ISymbol attributeSymbol,
		List<string> initializationLines)
	{
		AttributeData attributeData = field
			.GetAttributes()
			.Single(x => x.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true);

		INamedTypeSymbol? propertyType = attributeData.ConstructorArguments.SingleOrDefault().Value as INamedTypeSymbol;

		if (propertyType == null)
		{
			// TODO: Internal error.
			return;
		}

		string fieldName = field.Name;
		string fieldVisibility = SyntaxFacts.GetText(field.DeclaredAccessibility);
		string propertyName = fieldName.Substring(0, fieldName.Length - "PropertyKey".Length);
		string maybeNullPropertyTypeName = propertyType.ToDisplayString(NullableFlowState.MaybeNull);
		string propertyTypeName = propertyType.ToDisplayString();

		builder.AppendLine($"public static readonly {DependencyPropertyName} {propertyName}Property;");
		builder.AppendLine();

		AppendPropertyWithGetterSetter(
			builder,
			maybeNullPropertyTypeName,
			propertyName,
			fieldVisibility,
			isReadOnly: true);

		AppendCreatePropertyExpression(
			RegisterReadOnlyMethod,
			initializationLines,
			attributeData,
			classSymbol,
			propertyTypeName,
			propertyName,
			fieldName);

		initializationLines.Add($"{propertyName}Property = {fieldName}.{DependencyPropertyName};");
		initializationLines.Add("");
	}

	protected void ProcessAttachedProperty(
		CodeBuilder builder,
		INamedTypeSymbol classSymbol,
		IFieldSymbol field,
		ISymbol attributeSymbol,
		List<string> initializationLines)
	{
		AttributeData attributeData = field.GetAttributeData(attributeSymbol);
		INamedTypeSymbol? propertyType = attributeData.ConstructorArguments.SingleOrDefault().Value as INamedTypeSymbol;

		if (propertyType == null)
		{
			// TODO: Internal error.
			return;
		}

		string fieldName = field.Name;
		string fieldVisibility = SyntaxFacts.GetText(field.DeclaredAccessibility);
		string propertyName = fieldName.Substring(0, fieldName.Length - "Property".Length);
		string maybeNullPropertyTypeName = propertyType.ToDisplayString(NullableFlowState.MaybeNull);
		string propertyTypeName = propertyType.ToDisplayString();

		AddGetterSetterMethods(builder, maybeNullPropertyTypeName, propertyName, fieldVisibility);

		AppendCreatePropertyExpression(
			RegisterAttachedMethod,
			initializationLines,
			attributeData,
			classSymbol,
			propertyTypeName,
			propertyName,
			fieldName);
	}

	protected void ProcessAttachedPropertyKey(
		CodeBuilder builder,
		INamedTypeSymbol classSymbol,
		IFieldSymbol field,
		ISymbol attributeSymbol,
		List<string> initializationLines)
	{
		AttributeData attributeData = field
			.GetAttributes()
			.Single(x => x.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true);

		INamedTypeSymbol? propertyType = attributeData.ConstructorArguments.SingleOrDefault().Value as INamedTypeSymbol;

		if (propertyType == null)
		{
			// TODO: Internal error.
			return;
		}

		string fieldName = field.Name;
		string fieldVisibility = SyntaxFacts.GetText(field.DeclaredAccessibility);
		string propertyName = fieldName.Substring(0, fieldName.Length - "PropertyKey".Length);
		string maybeNullPropertyTypeName = propertyType.ToDisplayString(NullableFlowState.MaybeNull);
		string propertyTypeName = propertyType.ToDisplayString();

		builder.AppendLine($"public static readonly {DependencyPropertyName} {propertyName}Property;");
		builder.AppendLine();

		AddGetterSetterMethods(builder, maybeNullPropertyTypeName, propertyName, fieldVisibility);

		AppendCreatePropertyExpression(
			RegisterAttachedReadOnlyMethod,
			initializationLines,
			attributeData,
			classSymbol,
			propertyTypeName,
			propertyName,
			fieldName);

		initializationLines.Add($"{propertyName}Property = {fieldName}.{DependencyPropertyName};");
		initializationLines.Add("");
	}

	protected abstract IReadOnlyList<string> GetAdditionalParameters(AttributeData attributeData);

	protected void AppendPropertyWithGetterSetter(
		CodeBuilder builder,
		string propertyTypeName,
		string propertyName,
		string fieldVisibility,
		bool isReadOnly)
	{
		string propertySuffix = isReadOnly ? "Key" : "";

		// Do not append visibility if it is public.
		fieldVisibility = fieldVisibility.Replace("public", "");

		builder.AppendLine($"public {propertyTypeName} {propertyName}");
		builder.OpenScope();

		builder.AppendLine($"get => ({propertyTypeName})GetValue({propertyName}Property);");
		builder.AppendLine($"{fieldVisibility} set => SetValue({propertyName}Property{propertySuffix}, value);".Trim());

		builder.CloseScope();
		builder.AppendLine();
	}

	protected void AppendCreatePropertyExpression(
		string createMethod,
		List<string> initializationLines,
		AttributeData attributeData,
		INamedTypeSymbol classSymbol,
		string propertyTypeName,
		string propertyName,
		string fieldName)
	{
		IReadOnlyList<string> additionalParameters = GetAdditionalParameters(attributeData);

		initializationLines.Add($"{fieldName} = {DependencyPropertyName}.{createMethod}(");
		initializationLines.Add($"    \"{propertyName}\",");
		initializationLines.Add($"    typeof({propertyTypeName}),");
		initializationLines.Add($"    typeof({classSymbol.Name}),");

		for (int i = 0; i < additionalParameters.Count; i++)
		{
			string additionalParameter = i == additionalParameters.Count - 1
				? $"    {additionalParameters[i]});"
				: $"    {additionalParameters[i]},";

			initializationLines.Add(additionalParameter);
		}

		initializationLines.Add("");
	}

	protected void AddGetterSetterMethods(
		CodeBuilder builder,
		string propertyTypeName,
		string propertyName,
		string fieldVisibility)
	{
		builder.AppendLine($"public static {propertyTypeName} Get{propertyName}({BaseClassName} target)");
		builder.OpenScope();

		builder.AppendLine($"return ({propertyTypeName})target.GetValue({propertyName}Property);");

		builder.CloseScope();
		builder.AppendLine();

		builder.AppendLine($"{fieldVisibility} static void Set{propertyName}({BaseClassName} target, {propertyTypeName} value)");
		builder.OpenScope();

		builder.AppendLine($"target.SetValue({propertyName}Property, value);");

		builder.CloseScope();
		builder.AppendLine();
	}

	protected CheckResult CheckDependencyProperty(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		IFieldSymbol fieldSymbol,
		INamedTypeSymbol attributeSymbol)
	{
		CheckResult result = CheckResult.Valid;

		result = result.Combine(CheckThatClassHasBaseType(context, classSymbol, $"{PlatformNamespace}.{BaseClassName}", DoesNotInheritFromBaseClassDiagnosticDescriptor));
		result = result.Combine(CheckThatStaticConstructorDoesNotExist(context, classSymbol));
		result = result.Combine(CheckThatClassIsPartial(context, classSymbol));
		result = result.Combine(CheckFieldTypeAndName(context, fieldSymbol, FieldTypeAndNameConditions));
		result = result.Combine(CheckPropertyChangedMethodSignature(context, classSymbol, fieldSymbol, attributeSymbol, PropertyChangedMethodParameterTypes));
		result = result.Combine(CheckCoerceValueMethodSignature(context, classSymbol, fieldSymbol, attributeSymbol, CoerceValueMethodParameterTypes));
		result = result.Combine(CheckDefaultValueField(context, classSymbol, fieldSymbol, attributeSymbol));

		return result;
	}

	protected CheckResult CheckAttachedProperty(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		IFieldSymbol fieldSymbol,
		INamedTypeSymbol attributeSymbol)
	{
		CheckResult result = CheckResult.Valid;

		result = result.Combine(CheckThatStaticConstructorDoesNotExist(context, classSymbol));
		result = result.Combine(CheckThatClassIsPartial(context, classSymbol));
		result = result.Combine(CheckFieldTypeAndName(context, fieldSymbol, FieldTypeAndNameConditions));
		result = result.Combine(CheckPropertyChangedMethodSignature(context, classSymbol, fieldSymbol, attributeSymbol, PropertyChangedMethodParameterTypes));
		result = result.Combine(CheckCoerceValueMethodSignature(context, classSymbol, fieldSymbol, attributeSymbol, CoerceValueMethodParameterTypes));
		result = result.Combine(CheckDefaultValueField(context, classSymbol, fieldSymbol, attributeSymbol));

		return result;
	}

	protected CheckResult CheckThatClassHasBaseType(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		string expectedBaseType,
		DiagnosticDescriptor diagnosticDescriptor)
	{
		if (!InheritsFrom(classSymbol, expectedBaseType))
		{
			Diagnostic diagnostic = Diagnostic.Create(
				diagnosticDescriptor,
				classSymbol.Locations.FirstOrDefault() ?? Location.None,
				classSymbol.Name);

			context.ReportDiagnostic(diagnostic);
			return CheckResult.Invalid;
		}

		return CheckResult.Valid;

		bool InheritsFrom(INamedTypeSymbol? symbol, string baseTypeName)
		{
			while (symbol != null)
			{
				if (symbol.ToDisplayString() == baseTypeName)
				{
					return true;
				}

				symbol = symbol.BaseType;
			}

			return false;
		}
	}

	protected CheckResult CheckThatStaticConstructorDoesNotExist(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol)
	{
		if (classSymbol.Constructors.Any(x => x.IsStatic && !x.IsImplicitlyDeclared))
		{
			Diagnostic diagnostic = Diagnostic.Create(
				Diagnostics.ClassShouldNotHaveStaticConstructor,
				classSymbol.Locations.FirstOrDefault() ?? Location.None,
				classSymbol.Name);

			context.ReportDiagnostic(diagnostic);
			return CheckResult.Invalid;
		}

		return CheckResult.Valid;
	}

	protected CheckResult CheckThatClassIsPartial(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol)
	{
		IEnumerable<ClassDeclarationSyntax> classSyntaxes = classSymbol.DeclaringSyntaxReferences
			.Select(x => x.GetSyntax())
			.OfType<ClassDeclarationSyntax>();
		
		if (classSyntaxes.Any(x => x.Modifiers.All(y => !y.IsKind(SyntaxKind.PartialKeyword))))
		{
			Diagnostic diagnostic = Diagnostic.Create(
				Diagnostics.ClassShouldBePartial,
				classSymbol.Locations.FirstOrDefault() ?? Location.None,
				classSymbol.Name);

			context.ReportDiagnostic(diagnostic);
			return CheckResult.Invalid;
		}

		return CheckResult.Valid;
	}

	protected CheckResult CheckFieldTypeAndName(
		SourceProductionContext context,
		IFieldSymbol symbol,
		IReadOnlyList<(string FieldType, string FieldNameSuffix, DiagnosticDescriptor SuffixDiagnostic)> conditions)
	{
		string typeFullName = symbol.Type.ToDisplayString();
		string fieldName = symbol.Name;

		var matchedCondition = conditions.FirstOrDefault(x => typeFullName == x.FieldType);

		if (matchedCondition == default)
		{
			Diagnostic diagnostic = Diagnostic.Create(
				Diagnostics.IncorrectFieldType,
				symbol.Locations.FirstOrDefault() ?? Location.None,
				symbol.Name,
				string.Join(" or ", conditions.Select(x => x.FieldType)));

			context.ReportDiagnostic(diagnostic);
			return CheckResult.Invalid;
		}

		if (!fieldName.EndsWith(matchedCondition.FieldNameSuffix))
		{
			Diagnostic diagnostic = Diagnostic.Create(
				matchedCondition.SuffixDiagnostic,
				symbol.Locations.FirstOrDefault() ?? Location.None,
				symbol.Name);

			context.ReportDiagnostic(diagnostic);
			return CheckResult.Invalid;
		}

		return CheckResult.Valid;
	}

	protected CheckResult CheckPropertyChangedMethodSignature(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		IFieldSymbol fieldSymbol,
		INamedTypeSymbol attributeSymbol,
		IReadOnlyList<string> parameterTypes)
	{
		AttributeData attributeData = fieldSymbol.GetAttributeData(attributeSymbol);
		string? propertyChangedMethodName = attributeData.GetOnPropertyChangedMethod();

		if (propertyChangedMethodName == null)
		{
			return CheckResult.Valid;
		}

		IMethodSymbol? propertyChangedMethod = classSymbol
			.GetMembers()
			.OfType<IMethodSymbol>()
			.FirstOrDefault(x => x.Name == propertyChangedMethodName);

		if (propertyChangedMethod == null)
		{
			AddDiagnostic(Diagnostics.MissingPropertyChangedMethod);
			return CheckResult.Invalid;
		}

		if (!propertyChangedMethod.IsStatic)
		{
			AddDiagnostic();
			return CheckResult.Invalid;
		}

		if (propertyChangedMethod.ReturnType.SpecialType != SpecialType.System_Void)
		{
			AddDiagnostic();
			return CheckResult.Invalid;
		}

		ImmutableArray<IParameterSymbol> parameters = propertyChangedMethod.Parameters;

		if (!parameters.Select(x => x.Type.ToDisplayString()).SequenceEqual(parameterTypes))
		{
			AddDiagnostic();
			return CheckResult.Invalid;
		}

		return CheckResult.Valid;

		void AddDiagnostic(DiagnosticDescriptor? diagnosticDescriptor = null)
		{
			diagnosticDescriptor ??= Diagnostics.IncorrectPropertyChangedMethodSignature;

			Diagnostic diagnostic = Diagnostic.Create(
				diagnosticDescriptor,
				fieldSymbol.Locations.FirstOrDefault() ?? Location.None,
				propertyChangedMethodName,
				$"{propertyChangedMethodName}({string.Join(", ", PropertyChangedMethodParameterTypes)})");

			context.ReportDiagnostic(diagnostic);
		}
	}

	protected CheckResult CheckCoerceValueMethodSignature(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		IFieldSymbol fieldSymbol,
		INamedTypeSymbol attributeSymbol,
		IReadOnlyList<string> parameterTypes)
	{
		AttributeData attributeData = fieldSymbol.GetAttributeData(attributeSymbol);
		string? coerceValueMethodName = attributeData.GetOnCoerceValueMethod();

		if (coerceValueMethodName == null)
		{
			return CheckResult.Valid;
		}

		IMethodSymbol? coerceValueMethod = classSymbol
			.GetMembers()
			.OfType<IMethodSymbol>()
			.FirstOrDefault(x => x.Name == coerceValueMethodName);

		if (coerceValueMethod == null)
		{
			AddDiagnostic(Diagnostics.MissingCoerceValueMethod);
			return CheckResult.Invalid;
		}

		if (!coerceValueMethod.IsStatic)
		{
			AddDiagnostic();
			return CheckResult.Invalid;
		}

		if (coerceValueMethod.ReturnType.SpecialType != SpecialType.System_Object)
		{
			AddDiagnostic();
			return CheckResult.Invalid;
		}

		ImmutableArray<IParameterSymbol> parameters = coerceValueMethod.Parameters;

		if (!parameters.Select(x => x.Type.ToDisplayString()).SequenceEqual(parameterTypes))
		{
			AddDiagnostic();
			return CheckResult.Invalid;
		}

		return CheckResult.Valid;

		void AddDiagnostic(DiagnosticDescriptor? diagnosticDescriptor = null)
		{
			diagnosticDescriptor ??= Diagnostics.IncorrectCoerceValueMethodSignature;

			Diagnostic diagnostic = Diagnostic.Create(
				diagnosticDescriptor,
				fieldSymbol.Locations.FirstOrDefault() ?? Location.None,
				coerceValueMethodName,
				$"{coerceValueMethodName}({string.Join(", ", CoerceValueMethodParameterTypes)})");

			context.ReportDiagnostic(diagnostic);
		}
	}

	protected CheckResult CheckDefaultValueField(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		IFieldSymbol fieldSymbol,
		INamedTypeSymbol attributeSymbol)
	{
		AttributeData attributeData = fieldSymbol.GetAttributeData(attributeSymbol);
		string? defaultValueFieldName = attributeData.GetDefaultValueField();

		if (defaultValueFieldName == null)
		{
			return CheckResult.Valid;
		}

		IFieldSymbol? defaultValueField = classSymbol
			.GetMembers()
			.OfType<IFieldSymbol>()
			.FirstOrDefault(x => x.Name == defaultValueFieldName);

		TypedConstant propertyType = attributeData.ConstructorArguments.SingleOrDefault();

		if (defaultValueField == null)
		{
			AddDiagnostic(Diagnostics.MissingDefaultValueField);
			return CheckResult.Invalid;
		}

		if (!defaultValueField.IsStatic)
		{
			AddDiagnostic();
			return CheckResult.Invalid;
		}

		if (!defaultValueField.Type.Equals(propertyType.Value as INamedTypeSymbol, SymbolEqualityComparer.Default))
		{
			AddDiagnostic();
			return CheckResult.Invalid;
		}

		return CheckResult.Valid;

		void AddDiagnostic(DiagnosticDescriptor? diagnosticDescriptor = null)
		{
			diagnosticDescriptor ??= Diagnostics.IncorrectDefaultValueFieldDefinition;

			Diagnostic diagnostic = Diagnostic.Create(
				diagnosticDescriptor,
				fieldSymbol.Locations.FirstOrDefault() ?? Location.None,
				defaultValueFieldName,
				propertyType.Value);

			context.ReportDiagnostic(diagnostic);
		}
	}
}