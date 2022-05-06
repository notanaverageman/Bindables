using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Wpf.Test.Common;

[TestFixture]
public class IncorrectFieldName : WpfTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.IncorrectFieldName<DependencyPropertyGenerator>("DependencyProperty");
		this.IncorrectReadOnlyFieldName<DependencyPropertyGenerator>("DependencyProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.IncorrectFieldName<AttachedPropertyGenerator>("AttachedProperty");
		this.IncorrectReadOnlyFieldName<AttachedPropertyGenerator>("AttachedProperty");
	}
}