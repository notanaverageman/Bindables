using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Forms.Test.Common;

[TestFixture]
public class IncorrectDefaultValueFieldType : FormsTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.IncorrectDefaultValueFieldType<BindablePropertyGenerator>("BindableProperty");
		this.NonStaticDefaultValueField<BindablePropertyGenerator>("BindableProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.IncorrectDefaultValueFieldType<AttachedPropertyGenerator>("AttachedProperty");
		this.NonStaticDefaultValueField<AttachedPropertyGenerator>("AttachedProperty");
	}
}