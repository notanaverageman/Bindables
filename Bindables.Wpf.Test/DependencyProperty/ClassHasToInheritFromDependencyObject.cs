using System.Linq;
using Bindables.Test;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Bindables.Wpf.Test.DependencyProperty;

[TestFixture]
public class ClassHasToInheritFromDependencyObject : WpfTestBase
{
	private const string SourceCode = @"
using System.Windows;
using Bindables.Wpf;

public partial class InvalidClass
{
	[DependencyProperty(typeof(int))]
	public static readonly DependencyProperty ExampleProperty;
}";
	
	[Test]
	public void Test()
	{
		TestResult result = Generate<DependencyPropertyGenerator>(SourceCode);
		Diagnostic? error = result.Diagnostics.SingleOrDefault(x => x.Descriptor.Equals(Diagnostics.ClassDoesNotInheritFromDependencyObject));

		Assert.That(result.Diagnostics.Count, Is.EqualTo(1));
		Assert.That(error, Is.Not.Null);
	}
}