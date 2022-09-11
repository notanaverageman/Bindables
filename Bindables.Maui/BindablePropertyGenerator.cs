using Bindables.Xamarin;
using Microsoft.CodeAnalysis;

namespace Bindables.Maui;

[Generator]
public class BindablePropertyGenerator : XamarinPropertyGenerator
{
	public sealed override string AttributeName => "BindablePropertyAttribute";
	public sealed override string AttributeNamespace => MauiNamespaces.AttributeNamespace;
	public sealed override string PlatformNamespace => MauiNamespaces.PlatformNamespace;

	public sealed override bool IsAttachedPropertyGenerator => false;
}