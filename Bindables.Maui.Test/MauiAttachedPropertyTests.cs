using System.Collections.Generic;
using Bindables.Xamarin.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Maui.Test;

[TestFixture]
public class MauiAttachedPropertyTests : XamarinAttachedPropertyTests<AttachedPropertyGenerator>
{
	protected override IEnumerable<MetadataReference> GetAdditionalReferences()
	{
		return MauiMetadataReferences.Get();
	}
}