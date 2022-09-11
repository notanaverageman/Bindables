using Bindables.Windows;
using Microsoft.CodeAnalysis;

namespace Bindables.Wpf;

[Generator]
public class DependencyPropertyGenerator : WindowsPropertyGenerator
{
	public sealed override string AttributeName => "DependencyPropertyAttribute";
	public sealed override string AttributeNamespace => WpfNamespaces.AttributeNamespace;
	public sealed override string PlatformNamespace => WpfNamespaces.PlatformNamespace;

	public sealed override bool IsAttachedPropertyGenerator => false;
}