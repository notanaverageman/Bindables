using System.Collections.Generic;
using System.Windows;
using Microsoft.CodeAnalysis;

namespace Bindables.Wpf.Test;

public class WpfMetadataReferences
{
	public static IEnumerable<MetadataReference> Get()
	{
		yield return MetadataReference.CreateFromFile(typeof(DependencyObject).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(UIPropertyMetadata).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(FrameworkPropertyMetadata).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(FrameworkPropertyMetadataOptions).Assembly.Location);
	}
}