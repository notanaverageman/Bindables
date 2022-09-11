using System.Collections.Generic;
using Bindables.Xamarin.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Forms.Test;

[TestFixture]
public class FormsBindablePropertyTests : XamarinDependencyPropertyTests<BindablePropertyGenerator>
{
	protected override IEnumerable<MetadataReference> GetAdditionalReferences()
	{
		return FormsMetadataReferences.Get();
	}
}