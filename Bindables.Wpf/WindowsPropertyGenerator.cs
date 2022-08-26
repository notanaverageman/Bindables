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

#nullable enable

namespace Bindables.Wpf
{{
    [AttributeUsage(AttributeTargets.Field)]
    internal class {AttributeName} : Attribute
    {{
        public Type PropertyType {{ get; }}
        public string? OnPropertyChanged {{ get; set; }}
		public string? OnCoerceValue {{ get; set; }}
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
		string? coerceValueMethod = attributeData.GetOnCoerceValueMethod();
		string? defaultValueField = attributeData.GetDefaultValueField();
		string? options = attributeData.GetFrameworkPropertyMetadataOptions();

		string[] args = new string[4];
		int count = 0;

		if (defaultValueField != null || options != null)
		{
			args[count++] = defaultValueField ?? "default";
		}

		if (options != null)
		{
			args[count++] = $"(FrameworkPropertyMetadataOptions){options}";
		}

		if (propertyChangedMethod != null || coerceValueMethod != null)
		{
			args[count++] = propertyChangedMethod ?? "default";
		}

		if (coerceValueMethod != null)
		{
			args[count++] = coerceValueMethod;
		}

		return $"new FrameworkPropertyMetadata({string.Join(", ", args, 0, count)})";
	}
}