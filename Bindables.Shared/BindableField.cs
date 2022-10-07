using Microsoft.CodeAnalysis;

namespace Bindables;

public class BindableField
{
	public IFieldSymbol FieldSymbol { get; }
	public FieldProcessor FieldProcessor { get; }
	public PropertyType PropertyType { get; }

	public BindableField(IFieldSymbol fieldSymbol, FieldProcessor fieldProcessor, PropertyType propertyType)
	{
		FieldSymbol = fieldSymbol;
		FieldProcessor = fieldProcessor;
		PropertyType = propertyType;
	}
}