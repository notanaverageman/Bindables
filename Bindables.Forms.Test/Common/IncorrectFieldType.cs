using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Forms.Test.Common;

[TestFixture]
public class IncorrectFieldType : FormsTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.IncorrectFieldType<BindablePropertyGenerator>("BindableProperty");
		this.IncorrectReadOnlyFieldType<BindablePropertyGenerator>("BindableProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.IncorrectFieldType<AttachedPropertyGenerator>("AttachedProperty");
		this.IncorrectReadOnlyFieldType<AttachedPropertyGenerator>("AttachedProperty");
	}
}