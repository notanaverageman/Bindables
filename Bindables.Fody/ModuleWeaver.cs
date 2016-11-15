using System;
using System.Linq;
using Mono.Cecil;

namespace Bindables.Fody
{
	public class ModuleWeaver
	{
		private const string BindablesLibraryName = "Bindables";

		public Action<string> LogInfo { get; set; }
		public ModuleDefinition ModuleDefinition { get; set; }

		public ModuleWeaver()
		{
			LogInfo = m => { };
		}

		public void Execute()
		{
			AssemblyNameReference bindablesAssemblyReference = ModuleDefinition.AssemblyReferences.FirstOrDefault(reference => reference.Name == BindablesLibraryName);

			DependencyPropertyWeaver dependencyPropertyWeaver = new DependencyPropertyWeaver(ModuleDefinition);
			dependencyPropertyWeaver.Execute();

			ModuleDefinition.AssemblyReferences.Remove(bindablesAssemblyReference);
		}
	}
}