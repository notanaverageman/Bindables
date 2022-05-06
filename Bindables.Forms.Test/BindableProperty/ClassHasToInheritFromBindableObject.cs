using System.Linq;
using Bindables.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Forms.Test.BindableProperty;

[TestFixture]
public class ClassHasToInheritFromBindableObject : FormsTestBase
{
	private const string SourceCode = @"
using Bindables.Forms;
using Xamarin.Forms;

public partial class InvalidClass
{
	[BindableProperty(typeof(int))]
	public static readonly BindableProperty ExampleProperty;
}";
	
	[Test]
	public void Test()
	{
		TestResult result = Generate<BindablePropertyGenerator>(SourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.ClassDoesNotInheritFromDependencyObject));
		
		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}
}