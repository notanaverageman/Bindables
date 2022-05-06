using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Forms.Test.Common;

[TestFixture]
public class ClassShouldBePartial : FormsTestBase
{
	[Test]
	public void TestBindableProperty()
	{
		this.ClassShouldBePartial<BindablePropertyGenerator>("BindableProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.ClassShouldBePartial<AttachedPropertyGenerator>("AttachedProperty");
	}
}