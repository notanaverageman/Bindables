using System.Collections.Generic;
using System.Linq.Expressions;
using Bindables.Xamarin.Test;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Bindables.Maui.Test;

[TestFixture]
public class MauiPropertyTests : XamarinTests<MauiPropertyGenerator>
{
	protected override IEnumerable<MetadataReference> GetAdditionalReferences()
	{
		yield return MetadataReference.CreateFromFile(typeof(BindableObject).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location);
	}
}