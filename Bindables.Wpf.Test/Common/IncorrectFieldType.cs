using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Wpf.Test.Common;

[TestFixture]
public class IncorrectFieldType : WpfTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.IncorrectFieldType<DependencyPropertyGenerator>("DependencyProperty");
		this.IncorrectReadOnlyFieldType<DependencyPropertyGenerator>("DependencyProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.IncorrectFieldType<AttachedPropertyGenerator>("AttachedProperty");
		this.IncorrectReadOnlyFieldType<AttachedPropertyGenerator>("AttachedProperty");
	}
}