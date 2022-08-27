using Microsoft.CodeAnalysis;

namespace Bindables;

public class Diagnostics
{
	public static readonly DiagnosticDescriptor ClassDoesNotInheritFromDependencyObject = new(
		"BN001",
		"Class does not inherit from DependencyObject",
		"Class '{0}' does not inherit from DependencyObject",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor ClassDoesNotInheritFromBindableObject = new(
		"BN001",
		"Class does not inherit from BindableObject",
		"Class '{0}' does not inherit from BindableObject",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor ClassShouldNotHaveStaticConstructor = new(
		"BN003",
		"Class should not have static constructor",
		"Class '{0}' should not have static constructor as it will be used to initialize the DependencyProperty fields",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor ClassShouldBePartial = new(
		"BN004",
		"Class should be partial",
		"Class '{0}' should be partial",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor IncorrectFieldType = new(
		"BN005",
		"Incorrect field type",
		"Type of field '{0}' has to be {1}",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor IncorrectFieldName = new(
		"BN006",
		"Incorrect field name",
		"Name of field '{0}' has to end with Property",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor IncorrectReadOnlyFieldName = new(
		"BN007",
		"Incorrect field name",
		"Name of field '{0}' has to end with PropertyKey",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor MissingPropertyChangedMethod = new(
		"BN008",
		"Missing PropertyChanged method",
		"The property changed callback method 'static void {0}(DependencyObject, DependencyPropertyChangedEventArgs)' is not found",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor IncorrectPropertyChangedMethodSignature = new(
		"BN009",
		"Incorrect PropertyChanged method signature",
		"The signature of method '{0}.{1}' has to be 'static void {1}(DependencyObject, DependencyPropertyChangedEventArgs)'",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor MissingCoerceValueMethod = new(
		"BN008",
		"Missing CoerceValue method",
		"The coerce value callback method 'static void {0}(DependencyObject, object)' is not found",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor IncorrectCoerceValueMethodSignature = new(
		"BN009",
		"Incorrect CoerceValue method signature",
		"The signature of method '{0}.{1}' has to be 'static object {1}(DependencyObject, object)'",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor MissingDefaultValueField = new(
		"BN010",
		"Missing default value field",
		"The default value field 'static {1} {0}' is not found",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);

	public static readonly DiagnosticDescriptor IncorrectDefaultValueFieldDefinition = new(
		"BN011",
		"Incorrect default value field definition",
		"Default value field '{0}' should be static and of type '{1}'",
		"Code Generation",
		DiagnosticSeverity.Error,
		true);
}