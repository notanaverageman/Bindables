using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Bindables.Windows;

public abstract class WindowsPropertyGenerator : PropertyGeneratorBase
{
	public sealed override string DependencyPropertyName => "DependencyProperty";
	public sealed override string DependencyPropertyKeyName => "DependencyPropertyKey";

	public sealed override string DependencyPropertyAttributeName => "DependencyPropertyAttribute";
	public sealed override string AttachedPropertyAttributeName => "AttachedPropertyAttribute";

	public override string BaseClassName => "DependencyObject";
	public override string DerivedFromBaseClassName => $"{PlatformNamespace}.Controls.Button";
	
	public override string RegisterMethod => "Register";
	public override string RegisterReadOnlyMethod => "RegisterReadOnly";
	public override string RegisterAttachedMethod => "RegisterAttached";
	public override string RegisterAttachedReadOnlyMethod => "RegisterAttachedReadOnly";

	public override IReadOnlyList<string> PropertyChangedMethodParameterTypes { get; }
	public override IReadOnlyList<string> CoerceValueMethodParameterTypes { get; }

	public override DiagnosticDescriptor DoesNotInheritFromBaseClassDiagnosticDescriptor => Diagnostics.ClassDoesNotInheritFromDependencyObject;

	public sealed override string DependencyPropertyAttributeSourceText => GetAttributeSourceText(DependencyPropertyAttributeName);
	public sealed override string AttachedPropertyAttributeSourceText => GetAttributeSourceText(AttachedPropertyAttributeName);

	public WindowsPropertyGenerator()
	{
		// TODO: Use static interface
		PropertyChangedMethodParameterTypes = new[]
		{
			$"{PlatformNamespace}.{BaseClassName}",
			$"{PlatformNamespace}.DependencyPropertyChangedEventArgs"
		};

		CoerceValueMethodParameterTypes = new[]
		{
			$"{PlatformNamespace}.{BaseClassName}",
			"object"
		};
	}

	private string GetAttributeSourceText(string attributeName)
	{
		return $@"
using System;
using {PlatformNamespace};

#nullable enable

namespace {AttributeNamespace}
{{
    [AttributeUsage(AttributeTargets.Field)]
    internal class {attributeName} : Attribute
    {{
        public Type PropertyType {{ get; }}
        public string? OnPropertyChanged {{ get; set; }}
        public string? OnCoerceValue {{ get; set; }}
        public string? DefaultValueField {{ get; set; }}
        public FrameworkPropertyMetadataOptions Options {{ get; set; }}

        public {attributeName}(Type propertyType)
        {{
            PropertyType = propertyType;
        }}
    }}
}}
";
	}

	protected override IReadOnlyList<string> GetAdditionalParameters(AttributeData attributeData)
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

		return new[]
		{
			$"new FrameworkPropertyMetadata({string.Join(", ", args, 0, count)})"
		};
	}
}