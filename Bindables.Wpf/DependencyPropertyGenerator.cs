using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Bindables.Wpf;

[Generator]
public class DependencyPropertyGenerator : WindowsPropertyGenerator
{
	protected override string AttributeName => "DependencyPropertyAttribute";
	protected override string AttributeTypeName => "Bindables.Wpf.DependencyPropertyAttribute";

	protected override CheckResult Check(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		IFieldSymbol fieldSymbol,
		INamedTypeSymbol attributeSymbol)
	{
		(string, string, DiagnosticDescriptor)[] fieldTypeAndNameConditions =
		{
			("System.Windows.DependencyProperty", "Property", Diagnostics.IncorrectFieldName),
			("System.Windows.DependencyPropertyKey", "PropertyKey", Diagnostics.IncorrectReadOnlyFieldName),
		};

		string[] propertyChangedMethodParameterTypes =
		{
			"System.Windows.DependencyObject",
			"System.Windows.DependencyPropertyChangedEventArgs"
		};

		string[] coerceValueMethodParameterTypes =
		{
			"System.Windows.DependencyObject",
			"object",
		};

		CheckResult result = CheckResult.Valid;

		result = result.Combine(CheckThatClassHasBaseType(context, classSymbol, "System.Windows.DependencyObject", Diagnostics.ClassDoesNotInheritFromDependencyObject));
		result = result.Combine(CheckThatStaticConstructorDoesNotExist(context, classSymbol));
		result = result.Combine(CheckThatClassIsPartial(context, classSymbol));
		result = result.Combine(CheckFieldTypeAndName(context, fieldSymbol, fieldTypeAndNameConditions));
		result = result.Combine(CheckPropertyChangedMethodSignature(context, classSymbol, fieldSymbol, attributeSymbol, propertyChangedMethodParameterTypes));
		result = result.Combine(CheckCoerceValueMethodSignature(context, classSymbol, fieldSymbol, attributeSymbol, coerceValueMethodParameterTypes));
		result = result.Combine(CheckDefaultValueField(context, classSymbol, fieldSymbol, attributeSymbol));

		return result;
	}

	protected override void ProcessDependencyProperty(
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
		string propertyName = fieldName.Substring(0, fieldName.Length - "Property".Length);
		string maybeNullPropertyTypeName = propertyType.ToDisplayString(NullableFlowState.MaybeNull);
		string propertyTypeName = propertyType.ToDisplayString();

		builder.AppendLine($"public {maybeNullPropertyTypeName} {propertyName}");
		builder.OpenScope();

		builder.AppendLine($"get => ({maybeNullPropertyTypeName})GetValue({propertyName}Property);");
		builder.AppendLine($"set => SetValue({propertyName}Property, value);");

		builder.CloseScope();
		builder.AppendLine();

		string metadataDefinition = GetMetadataDefinition(attributeData);

		initializationLines.Add($"{fieldName} = DependencyProperty.Register(");
		initializationLines.Add($"    nameof({propertyName}),");
		initializationLines.Add($"    typeof({propertyTypeName}),");
		initializationLines.Add($"    typeof({classSymbol.Name}),");
		initializationLines.Add($"    {metadataDefinition});");
		initializationLines.Add("");
	}

	protected override void ProcessDependencyPropertyKey(
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
		string propertyName = fieldName.Substring(0, fieldName.Length - "PropertyKey".Length);
		string maybeNullPropertyTypeName = propertyType.ToDisplayString(NullableFlowState.MaybeNull);
		string propertyTypeName = propertyType.ToDisplayString();

		builder.AppendLine($"public static readonly DependencyProperty {propertyName}Property;");
		builder.AppendLine();

		builder.AppendLine($"public {maybeNullPropertyTypeName} {propertyName}");
		builder.OpenScope();

		builder.AppendLine($"get => ({maybeNullPropertyTypeName})GetValue({propertyName}Property);");
		builder.AppendLine($"private set => SetValue({propertyName}PropertyKey, value);");

		builder.CloseScope();
		builder.AppendLine();

		string metadataDefinition = GetMetadataDefinition(attributeData);

		initializationLines.Add($"{fieldName} = DependencyProperty.RegisterReadOnly(");
		initializationLines.Add($"    nameof({propertyName}),");
		initializationLines.Add($"    typeof({propertyTypeName}),");
		initializationLines.Add($"    typeof({classSymbol.Name}),");
		initializationLines.Add($"    {metadataDefinition});");
		initializationLines.Add("");

		initializationLines.Add($"{propertyName}Property = {fieldName}.DependencyProperty;");
		initializationLines.Add("");
	}
}