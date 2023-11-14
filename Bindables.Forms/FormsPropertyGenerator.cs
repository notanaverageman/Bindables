using Bindables.Xamarin;
using Microsoft.CodeAnalysis;

namespace Bindables.Forms;

[Generator]
public class FormsPropertyGenerator : XamarinPropertyGenerator
{
	public sealed override string AttributeNamespace => "Bindables.Forms";
	public sealed override string PlatformNamespace => "Xamarin.Forms";
	public sealed override string GeneratorName => typeof(FormsPropertyGenerator).FullName ??
	                                               $"{AttributeNamespace}.{nameof(FormsPropertyGenerator)}";
	public sealed override string GeneratorVersion => typeof(FormsPropertyGenerator).Assembly.GetName().Version.ToString();
}