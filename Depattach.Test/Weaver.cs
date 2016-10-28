using System.IO;
using System.Reflection;
using Depattach.Fody;
using Mono.Cecil;

namespace Depattach.Test
{
	public class Weaver
	{
		public const string DependencyProperty = "AssemblyDependencyProperty.dll";
		public const string DependencyPropertyDefaultValue = "AssemblyDependencyProperty.DefaultValue.dll";

		public static Assembly Weave(string assemblyName)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				ModuleDefinition module = ModuleDefinition.ReadModule(assemblyName);

				ModuleWeaver weavingTask = new ModuleWeaver
				{
					ModuleDefinition = module
				};

				weavingTask.Execute();

				module.Write(stream);
				stream.Seek(0, SeekOrigin.Begin);

				return Assembly.Load(stream.ToArray());
			}
		}
	}
}