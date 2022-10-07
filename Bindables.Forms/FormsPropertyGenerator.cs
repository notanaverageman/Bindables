using Bindables.Xamarin;
using Microsoft.CodeAnalysis;

namespace Bindables.Forms;

[Generator]
public class FormsPropertyGenerator : XamarinPropertyGenerator
{
	public sealed override string AttributeNamespace => "Bindables.Forms";
	public sealed override string PlatformNamespace => "Xamarin.Forms";
}