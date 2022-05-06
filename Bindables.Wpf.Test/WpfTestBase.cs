using System.Collections.Generic;
using System.Windows;
using Bindables.Test;
using Microsoft.CodeAnalysis;

namespace Bindables.Wpf.Test;

public class WpfTestBase : TestBase
{
	public override string AttributeNamespace => "Bindables.Wpf";
	public override string PlatformNamespace => "System.Windows";
	public override string BaseClassName => "DependencyObject";
	public override string DependencyPropertyName => "DependencyProperty";
	public override string DependencyPropertyKeyName => "DependencyPropertyKey";
	public override string[] PropertyChangedMethodParameterTypes => new[]
	{
		"DependencyObject",
		"DependencyPropertyChangedEventArgs"
	};

	protected override IEnumerable<MetadataReference> GetAdditionalReferences()
	{
		yield return MetadataReference.CreateFromFile(typeof(DependencyObject).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(UIPropertyMetadata).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(FrameworkPropertyMetadata).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(FrameworkPropertyMetadataOptions).Assembly.Location);
	}
}