using Bindables.Windows;
using Microsoft.CodeAnalysis;

namespace Bindables.Wpf;

[Generator]
public class WpfPropertyGenerator : WindowsPropertyGenerator
{
	public sealed override string AttributeNamespace => "Bindables.Wpf";
	public sealed override string PlatformNamespace => "System.Windows";
	public sealed override string GeneratorName => typeof(WpfPropertyGenerator).FullName ??
	                                               $"{AttributeNamespace}.{nameof(WpfPropertyGenerator)}";
	public sealed override string GeneratorVersion => typeof(WpfPropertyGenerator).Assembly.GetName().Version.ToString();
}