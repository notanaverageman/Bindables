using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Bindables.Forms;

public abstract class XamarinPropertyGenerator : PropertyGeneratorBase
{
	protected sealed override string PlatformNamespace => "Xamarin.Forms";
	protected sealed override string DependencyPropertyName => "BindableProperty";
	protected sealed override string DependencyPropertyKeyName => "BindablePropertyKey";

	protected sealed override string AttributeSourceText => $@"
using System;
using Xamarin.Forms;

#nullable enable

namespace Bindables.Forms
{{
    [AttributeUsage(AttributeTargets.Field)]
    internal class {AttributeName} : Attribute
    {{
        public Type PropertyType {{ get; }}
        public string? OnPropertyChanged {{ get; set; }}
        public string? DefaultValueField {{ get; set; }}
        public BindingMode BindingMode {{ get; set; }}

        public {AttributeName}(Type propertyType)
        {{
            PropertyType = propertyType;
        }}
    }}
}}
";

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

		initializationLines.Add($"{fieldName} = BindableProperty.{createMethod}(");
		initializationLines.Add($"    nameof({propertyName}),");
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

	protected IReadOnlyList<string> GetAdditionalParameters(AttributeData attributeData)
	{
		string? propertyChangedMethod = attributeData.GetOnPropertyChangedMethod();
		string? defaultValueField = attributeData.GetDefaultValueField();
		string? bindingMode = attributeData.GetBindingMode();

		List<string> additionalParameters = new();

		string defaultValue = defaultValueField ?? "default";

		additionalParameters.Add(defaultValue);


		if (bindingMode != null)
		{
			additionalParameters.Add($"defaultBindingMode: (BindingMode){bindingMode}");
		}

		if (propertyChangedMethod != null)
		{
			additionalParameters.Add($"propertyChanged: {propertyChangedMethod}");
		}

		return additionalParameters;
	}
}