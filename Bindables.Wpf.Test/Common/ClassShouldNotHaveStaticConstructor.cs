using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Wpf.Test.Common;

[TestFixture]
public class ClassShouldNotHaveStaticConstructor : WpfTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.ClassShouldNotHaveStaticConstructor<DependencyPropertyGenerator>("DependencyProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.ClassShouldNotHaveStaticConstructor<AttachedPropertyGenerator>("AttachedProperty");
	}
}