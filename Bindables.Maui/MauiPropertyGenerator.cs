using Bindables.Xamarin;
using Microsoft.CodeAnalysis;

namespace Bindables.Maui;

[Generator]
public class MauiPropertyGenerator : XamarinPropertyGenerator
{
	public sealed override string AttributeNamespace => "Bindables.Maui";
	public sealed override string PlatformNamespace => "Microsoft.Maui.Controls";
}