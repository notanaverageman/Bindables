using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bindables.Forms;

[Generator]
public class BindablePropertyGenerator : XamarinPropertyGenerator
{
	protected override string AttributeName => "BindablePropertyAttribute";
	protected override string AttributeTypeName => "Bindables.Forms.BindablePropertyAttribute";

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

		result = result.Combine(CheckThatClassHasBaseType(context, classSymbol, "Xamarin.Forms.BindableObject", Diagnostics.ClassDoesNotInheritFromBindableObject));
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
			"Create",
			initializationLines,
			attributeData,
			classSymbol,
			propertyTypeName,
			propertyName,
			fieldName);
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
		string fieldVisibility = SyntaxFacts.GetText(field.DeclaredAccessibility);
		string propertyName = fieldName.Substring(0, fieldName.Length - "PropertyKey".Length);
		string maybeNullPropertyTypeName = propertyType.ToDisplayString(NullableFlowState.MaybeNull);
		string propertyTypeName = propertyType.ToDisplayString();

		builder.AppendLine($"public static readonly BindableProperty {propertyName}Property;");
		builder.AppendLine();

		AppendPropertyWithGetterSetter(
			builder,
			maybeNullPropertyTypeName,
			propertyName,
			fieldVisibility,
			isReadOnly: true);

		AppendCreatePropertyExpression(
			"CreateReadOnly",
			initializationLines,
			attributeData,
			classSymbol,
			propertyTypeName,
			propertyName,
			fieldName);

		initializationLines.Add($"{propertyName}Property = {fieldName}.BindableProperty;");
		initializationLines.Add("");
	}
}