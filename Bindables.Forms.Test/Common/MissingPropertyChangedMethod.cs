using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Forms.Test.Common;

[TestFixture]
public class MissingPropertyChangedMethod : FormsTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.MissingPropertyChangedMethod<BindablePropertyGenerator>("BindableProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.MissingPropertyChangedMethod<AttachedPropertyGenerator>("AttachedProperty");
	}
}