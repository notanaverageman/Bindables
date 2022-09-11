using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls;

namespace Bindables.Maui.Test;

public class MauiMetadataReferences
{
	public static IEnumerable<MetadataReference> Get()
	{
		yield return MetadataReference.CreateFromFile(typeof(BindableObject).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location);
	}
}