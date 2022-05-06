using System.Linq;
using Microsoft.CodeAnalysis;

namespace Bindables;

public static class AttributeExtensions
{
	public static AttributeData GetAttributeData(this IFieldSymbol field, ISymbol attributeSymbol)
	{
		return field
			.GetAttributes()
			.Single(x => x.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true);
	}

	public static string? GetOnPropertyChangedMethod(this AttributeData attributeData)
	{
		TypedConstant typedConstant = attributeData.NamedArguments.SingleOrDefault(x => x.Key == "OnPropertyChanged").Value;
		return typedConstant.Value?.ToString();
	}

	public static string? GetDefaultValueField(this AttributeData attributeData)
	{
		TypedConstant typedConstant = attributeData.NamedArguments.SingleOrDefault(x => x.Key == "DefaultValueField").Value;
		return typedConstant.Value?.ToString();
	}

	public static string? GetFrameworkPropertyMetadataOptions(this AttributeData attributeData)
	{
		TypedConstant typedConstant = attributeData.NamedArguments.SingleOrDefault(x => x.Key == "Options").Value;
		return typedConstant.Value?.ToString();
	}

	public static string? GetBindingMode(this AttributeData attributeData)
	{
		TypedConstant typedConstant = attributeData.NamedArguments.SingleOrDefault(x => x.Key == "BindingMode").Value;
		return typedConstant.Value?.ToString();
	}
}