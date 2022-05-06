using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Wpf.Test.Common;

[TestFixture]
public class IncorrectPropertyChangedMethodSignature : WpfTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.IncorrectPropertyChangedMethodSignatureInvalidParameters<DependencyPropertyGenerator>("DependencyProperty");
		this.IncorrectPropertyChangedMethodSignatureNonStatic<DependencyPropertyGenerator>("DependencyProperty");
		this.IncorrectPropertyChangedMethodSignatureReturnType<DependencyPropertyGenerator>("DependencyProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.IncorrectPropertyChangedMethodSignatureInvalidParameters<AttachedPropertyGenerator>("AttachedProperty");
		this.IncorrectPropertyChangedMethodSignatureNonStatic<AttachedPropertyGenerator>("AttachedProperty");
		this.IncorrectPropertyChangedMethodSignatureReturnType<AttachedPropertyGenerator>("AttachedProperty");
	}
}