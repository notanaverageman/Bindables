using System.Collections.Generic;
using Bindables.Windows.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Wpf.Test;

[TestFixture]
public class WpfAttachedPropertyTests : WindowsAttachedPropertyTests<AttachedPropertyGenerator>
{
	protected override IEnumerable<MetadataReference> GetAdditionalReferences()
	{
		return WpfMetadataReferences.Get();
	}
}