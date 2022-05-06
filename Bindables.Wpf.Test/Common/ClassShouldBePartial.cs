using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Wpf.Test.Common;

[TestFixture]
public class ClassShouldBePartial : WpfTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.ClassShouldBePartial<DependencyPropertyGenerator>("DependencyProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.ClassShouldBePartial<AttachedPropertyGenerator>("AttachedProperty");
	}
}