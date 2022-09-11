using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Xamarin.Forms;

namespace Bindables.Forms.Test;

public class FormsMetadataReferences
{
	public static IEnumerable<MetadataReference> Get()
	{
		yield return MetadataReference.CreateFromFile(typeof(BindableObject).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location);
	}
}