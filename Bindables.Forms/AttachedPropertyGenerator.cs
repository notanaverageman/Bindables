using Bindables.Xamarin;
using Microsoft.CodeAnalysis;

namespace Bindables.Forms;

[Generator]
public class AttachedPropertyGenerator : XamarinPropertyGenerator
{
	public sealed override string AttributeName => "AttachedPropertyAttribute";
	public sealed override string AttributeNamespace => FormsNamespaces.AttributeNamespace;
	public sealed override string PlatformNamespace => FormsNamespaces.PlatformNamespace;

	public sealed override bool IsAttachedPropertyGenerator => true;
}