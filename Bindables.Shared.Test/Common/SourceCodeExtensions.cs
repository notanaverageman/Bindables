namespace Bindables.Test;

public static class SourceCodeExtensions
{
	public static string ReplacePlaceholders(this string sourceCode, TestBase testBase, string attributeName)
	{
		return sourceCode
			.Replace("PlatformNamespace", testBase.PlatformNamespace)
			.Replace("AttributeNamespace", testBase.AttributeNamespace)
			.Replace("BaseClassName", testBase.BaseClassName)
			.Replace("AttributeName", attributeName)
			.Replace("KeyPropertyType", testBase.DependencyPropertyKeyName)
			.Replace("PropertyType", testBase.DependencyPropertyName);

	}
}