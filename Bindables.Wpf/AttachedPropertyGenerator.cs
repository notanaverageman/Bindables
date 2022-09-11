using Bindables.Windows;
using Microsoft.CodeAnalysis;

namespace Bindables.Wpf;

[Generator]
public class AttachedPropertyGenerator : WindowsPropertyGenerator
{
	public sealed override string AttributeName => "AttachedPropertyAttribute";
	public sealed override string AttributeNamespace => WpfNamespaces.AttributeNamespace;
	public sealed override string PlatformNamespace => WpfNamespaces.PlatformNamespace;

	public sealed override bool IsAttachedPropertyGenerator => true;
}