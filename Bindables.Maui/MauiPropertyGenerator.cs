using Bindables.Xamarin;
using Microsoft.CodeAnalysis;

namespace Bindables.Maui;

[Generator]
public class MauiPropertyGenerator : XamarinPropertyGenerator
{
	public sealed override string AttributeNamespace => "Bindables.Maui";
	public sealed override string PlatformNamespace => "Microsoft.Maui.Controls";
	public sealed override string GeneratorName => typeof(MauiPropertyGenerator).FullName ??
	                                               $"{AttributeNamespace}.{nameof(MauiPropertyGenerator)}";
	public sealed override string GeneratorVersion => typeof(MauiPropertyGenerator).Assembly.GetName().Version.ToString();
}