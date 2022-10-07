using System.Collections.Generic;
using System.Linq.Expressions;
using Bindables.Xamarin.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Xamarin.Forms;

namespace Bindables.Forms.Test;

[TestFixture]
public class FormsPropertyTests : XamarinTests<FormsPropertyGenerator>
{
	protected override IEnumerable<MetadataReference> GetAdditionalReferences()
	{
		yield return MetadataReference.CreateFromFile(typeof(BindableObject).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location);
	}
}