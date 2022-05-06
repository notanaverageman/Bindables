using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Wpf.Test.Common;

[TestFixture]
public class MissingDefaultValueField : WpfTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.MissingDefaultValueField<DependencyPropertyGenerator>("DependencyProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.MissingDefaultValueField<AttachedPropertyGenerator>("AttachedProperty");
	}
}