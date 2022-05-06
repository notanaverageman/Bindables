using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Forms.Test.Common;

[TestFixture]
public class IncorrectFieldName : FormsTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.IncorrectFieldName<BindablePropertyGenerator>("BindableProperty");
		this.IncorrectReadOnlyFieldName<BindablePropertyGenerator>("BindableProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.IncorrectFieldName<AttachedPropertyGenerator>("AttachedProperty");
		this.IncorrectReadOnlyFieldName<AttachedPropertyGenerator>("AttachedProperty");
	}
}