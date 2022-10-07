using Bindables.Windows;
using Microsoft.CodeAnalysis;

namespace Bindables.Wpf;

[Generator]
public class WpfPropertyGenerator : WindowsPropertyGenerator
{
	public sealed override string AttributeNamespace => "Bindables.Wpf";
	public sealed override string PlatformNamespace => "System.Windows";
}