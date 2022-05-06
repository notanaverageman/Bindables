using Microsoft.CodeAnalysis;

namespace Bindables.Wpf;

public abstract class WindowsPropertyGenerator : PropertyGeneratorBase
{
	protected sealed override string PlatformNamespace => "System.Windows";
	protected sealed override string DependencyPropertyName => "DependencyProperty";
	protected sealed override string DependencyPropertyKeyName => "DependencyPropertyKey";

	protected sealed override string AttributeSourceText => $@"
using System;
using System.Windows;

namespace Bindables.Wpf
{{
    [AttributeUsage(AttributeTargets.Field)]
    internal class {AttributeName} : Attribute
    {{
        public Type PropertyType {{ get; }}
        public string? OnPropertyChanged {{ get; set; }}
        public string? DefaultValueField {{ get; set; }}
        public FrameworkPropertyMetadataOptions Options {{ get; set; }}

        public {AttributeName}(Type propertyType)
        {{
            PropertyType = propertyType;
        }}
    }}
}}
";

	protected string GetMetadataDefinition(AttributeData attributeData)
	{
		string? propertyChangedMethod = attributeData.GetOnPropertyChangedMethod();
		string? defaultValueField = attributeData.GetDefaultValueField();
		string? options = attributeData.GetFrameworkPropertyMetadataOptions();

		if (propertyChangedMethod == null && defaultValueField == null && options == null)
		{
			return "new PropertyMetadata()";
		}

		string definition = options == null
			? "new PropertyMetadata("
			: "new FrameworkPropertyMetadata(";

		if (defaultValueField != null)
		{
			definition += defaultValueField;
		}

		if (options != null)
		{
			if (defaultValueField == null)
			{
				definition += "default";
			}

			definition += ", ";
			definition += $"(FrameworkPropertyMetadataOptions){options}";
		}

		if (propertyChangedMethod != null)
		{
			if (defaultValueField != null || options != null)
			{
				definition += ", ";
			}

			definition += propertyChangedMethod;
		}

		definition += ")";


		return definition;
	}
}