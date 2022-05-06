using System.Collections.Generic;
using System.Linq.Expressions;
using Bindables.Test;
using Microsoft.CodeAnalysis;
using Xamarin.Forms;

namespace Bindables.Forms.Test;

public class FormsTestBase : TestBase
{
	public override string AttributeNamespace => "Bindables.Forms";
	public override string PlatformNamespace => "Xamarin.Forms";
	public override string BaseClassName => "BindableObject";
	public override string DependencyPropertyName => "BindableProperty";
	public override string DependencyPropertyKeyName => "BindablePropertyKey";
	public override string[] PropertyChangedMethodParameterTypes => new[]
	{
		"BindableObject",
		"object",
		"object"
	};

	protected override IEnumerable<MetadataReference> GetAdditionalReferences()
	{
		yield return MetadataReference.CreateFromFile(typeof(BindableObject).Assembly.Location);
		yield return MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location);
	}
}