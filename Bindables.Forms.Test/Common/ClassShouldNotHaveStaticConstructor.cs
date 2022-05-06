using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Forms.Test.Common;

[TestFixture]
public class ClassShouldNotHaveStaticConstructor : FormsTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.ClassShouldNotHaveStaticConstructor<BindablePropertyGenerator>("BindableProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.ClassShouldNotHaveStaticConstructor<AttachedPropertyGenerator>("AttachedProperty");
	}
}