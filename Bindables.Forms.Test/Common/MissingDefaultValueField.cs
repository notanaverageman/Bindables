using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Forms.Test.Common;

[TestFixture]
public class MissingDefaultValueField : FormsTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.MissingDefaultValueField<BindablePropertyGenerator>("BindableProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.MissingDefaultValueField<AttachedPropertyGenerator>("AttachedProperty");
	}
}