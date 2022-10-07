using System.Collections.Generic;
using System.Windows;
using Bindables.Windows.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Wpf.Test;

[TestFixture]
public class WpfPropertyTests : WindowsTests<WpfPropertyGenerator>
{
	protected override IEnumerable<MetadataReference> GetAdditionalReferences()
	{
		yield return MetadataReference.CreateFromFile(typeof(DependencyObject).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(UIPropertyMetadata).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(FrameworkPropertyMetadata).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(FrameworkPropertyMetadataOptions).Assembly.Location);
	}
}