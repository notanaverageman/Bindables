using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bindables.Forms;

[Generator]
public class AttachedPropertyGenerator : XamarinPropertyGenerator
{
	protected override string AttributeName => "AttachedPropertyAttribute";
	protected override string AttributeTypeName => "Bindables.Forms.AttachedPropertyAttribute";

	protected override CheckResult Check(
		SourceProductionContext context,
		INamedTypeSymbol classSymbol,
		IFieldSymbol fieldSymbol,
		INamedTypeSymbol attributeSymbol)
	{
		(string, string, DiagnosticDescriptor)[] fieldTypeAndNameConditions =
		{
			("Xamarin.Forms.BindableProperty", "Property", Diagnostics.IncorrectFieldName),
			("Xamarin.Forms.BindablePropertyKey", "PropertyKey", Diagnostics.IncorrectReadOnlyFieldName),
		};

		string[] propertyChangedMethodParameterTypes =
		{
			"Xamarin.Forms.BindableObject",
			"object",
			"object"
		};

		CheckResult result = CheckResult.Valid;
		
		result = result.Combine(CheckThatStaticConstructorDoesNotExist(context, classSymbol));
		result = result.Combine(CheckThatClassIsPartial(context, classSymbol));
		result = result.Combine(CheckFieldTypeAndName(context, fieldSymbol, fieldTypeAndNameConditions));
		result = result.Combine(CheckPropertyChangedMethodSignature(context, classSymbol, fieldSymbol, attributeSymbol, propertyChangedMethodParameterTypes));
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
		TypedConstant propertyType = attributeData.ConstructorArguments.SingleOrDefault();

		string fieldName = field.Name;
		string fieldVisibility = SyntaxFacts.GetText(field.DeclaredAccessibility);
		string propertyName = fieldName.Substring(0, fieldName.Length - "Property".Length);
		string? propertyTypeName = propertyType.Value?.ToString();

		if (propertyTypeName == null)
		{
			// TODO: Internal error.
			return;
		}

		AddGetterSetterMethods(builder, propertyTypeName, propertyName, fieldVisibility);

		initializationLines.Add($"{fieldName} = DependencyProperty.RegisterAttached(");
		initializationLines.Add($"    \"{propertyName}\",");
		initializationLines.Add($"    typeof({propertyTypeName}),");
		initializationLines.Add($"    typeof({classSymbol.Name});");
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

		TypedConstant propertyType = attributeData.ConstructorArguments.SingleOrDefault();

		string fieldName = field.Name;
		string fieldVisibility = SyntaxFacts.GetText(field.DeclaredAccessibility);
		string propertyName = fieldName.Substring(0, fieldName.Length - "PropertyKey".Length);
		string? propertyTypeName = propertyType.Value?.ToString();

		if (propertyTypeName == null)
		{
			// TODO: Internal error.
			return;
		}

		builder.AppendLine($"public static readonly DependencyProperty {propertyName}Property;");
		builder.AppendLine();

		AddGetterSetterMethods(builder, propertyTypeName, propertyName, fieldVisibility);
		
		initializationLines.Add($"{fieldName} = DependencyProperty.RegisterAttachedReadOnly(");
		initializationLines.Add($"    \"{propertyName}\",");
		initializationLines.Add($"    typeof({propertyTypeName}),");
		initializationLines.Add($"    typeof({classSymbol.Name});");
		initializationLines.Add("");

		initializationLines.Add($"{propertyName}Property = {fieldName}.DependencyProperty;");
		initializationLines.Add("");
	}

	private void AddGetterSetterMethods(
		CodeBuilder builder,
		string propertyTypeName,
		string propertyName,
		string fieldVisibility)
	{
		builder.AppendLine($"public static {propertyTypeName} Get{propertyName}(DependencyObject target)");
		builder.OpenScope();

		builder.AppendLine($"return ({propertyTypeName})target.GetValue({propertyName}Property);");

		builder.CloseScope();
		builder.AppendLine();

		builder.AppendLine($"{fieldVisibility} static void Set{propertyName}(DependencyObject target, {propertyTypeName} value)");
		builder.OpenScope();

		builder.AppendLine($"target.SetValue({propertyName}Property, value);");

		builder.CloseScope();
		builder.AppendLine();
	}
}