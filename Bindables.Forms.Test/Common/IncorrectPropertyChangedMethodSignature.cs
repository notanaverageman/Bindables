using Bindables.Test;
using NUnit.Framework;

namespace Bindables.Forms.Test.Common;

[TestFixture]
public class IncorrectPropertyChangedMethodSignature : FormsTestBase
{
	[Test]
	public void TestDependencyProperty()
	{
		this.IncorrectPropertyChangedMethodSignatureInvalidParameters<BindablePropertyGenerator>("BindableProperty");
		this.IncorrectPropertyChangedMethodSignatureNonStatic<BindablePropertyGenerator>("BindableProperty");
		this.IncorrectPropertyChangedMethodSignatureReturnType<BindablePropertyGenerator>("BindableProperty");
	}

	[Test]
	public void TestAttachedProperty()
	{
		this.IncorrectPropertyChangedMethodSignatureInvalidParameters<AttachedPropertyGenerator>("AttachedProperty");
		this.IncorrectPropertyChangedMethodSignatureNonStatic<AttachedPropertyGenerator>("AttachedProperty");
		this.IncorrectPropertyChangedMethodSignatureReturnType<AttachedPropertyGenerator>("AttachedProperty");
	}
}