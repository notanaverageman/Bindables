using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Wpf.Test.Common;

[TestFixture]
public class MissingPropertyChangedMethod : WpfTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.MissingPropertyChangedMethod<DependencyPropertyGenerator>("DependencyProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.MissingPropertyChangedMethod<AttachedPropertyGenerator>("AttachedProperty");
	}
}