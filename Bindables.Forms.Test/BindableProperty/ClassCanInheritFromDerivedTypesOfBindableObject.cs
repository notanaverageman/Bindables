using System.Linq;
using Bindables.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Forms.Test.BindableProperty;

[TestFixture]
public class ClassCanInheritFromDerivedTypesOfBindableObject : FormsTestBase
{
	private const string SourceCode = @"
using Bindables.Forms;
using Xamarin.Forms;

public partial class ExampleClass : View
{
	[BindableProperty(typeof(int))]
	public static readonly BindableProperty ExampleProperty;
}";
	
	[Test]
	public void Test()
	{
		TestResult result = Generate<BindablePropertyGenerator>(SourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.ClassDoesNotInheritFromBindableObject));

		Assert.That(result.Diagnostics, Is.Empty);
		Assert.That(error, Is.Null);
	}
}