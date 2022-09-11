using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Bindables.Xamarin;

public abstract class XamarinPropertyGenerator : PropertyGeneratorBase
{
	public sealed override string DependencyPropertyName => "BindableProperty";
	public sealed override string DependencyPropertyKeyName => "BindablePropertyKey";

	public override string BaseClassName => "BindableObject";
	public override string DerivedFromBaseClassName => "Button";

	public override string RegisterMethod => "Create";
	public override string RegisterReadOnlyMethod => "CreateReadOnly";
	public override string RegisterAttachedMethod => "CreateAttached";
	public override string RegisterAttachedReadOnlyMethod => "CreateAttachedReadOnly";

	public override IReadOnlyList<string> PropertyChangedMethodParameterTypes { get; }
	public override IReadOnlyList<string> CoerceValueMethodParameterTypes { get; }

	public override DiagnosticDescriptor DoesNotInheritFromBaseClassDiagnosticDescriptor => Diagnostics.ClassDoesNotInheritFromBindableObject;

	public sealed override string AttributeSourceText => $@"
using System;
using {PlatformNamespace};

#nullable enable

namespace {AttributeNamespace}
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

	public XamarinPropertyGenerator()
	{
		// TODO: Use static interface
		PropertyChangedMethodParameterTypes = new[]
		{
			$"{PlatformNamespace}.{BaseClassName}",
			"object",
			"object"
		};

		CoerceValueMethodParameterTypes = new[]
		{
			$"{PlatformNamespace}.{BaseClassName}",
			"object"
		};
	}
	
	protected override IReadOnlyList<string> GetAdditionalParameters(AttributeData attributeData)
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