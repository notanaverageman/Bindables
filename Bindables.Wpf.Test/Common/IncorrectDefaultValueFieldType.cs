using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Wpf.Test.Common;

[TestFixture]
public class IncorrectDefaultValueFieldType : WpfTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.IncorrectDefaultValueFieldType<DependencyPropertyGenerator>("DependencyProperty");
		this.NonStaticDefaultValueField<DependencyPropertyGenerator>("DependencyProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.IncorrectDefaultValueFieldType<AttachedPropertyGenerator>("AttachedProperty");
		this.NonStaticDefaultValueField<AttachedPropertyGenerator>("AttachedProperty");
	}
}