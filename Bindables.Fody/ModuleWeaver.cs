using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

namespace Bindables.Fody
{
	public class ModuleWeaver: BaseModuleWeaver
	{
		public override void Execute()
		{
			DependencyPropertyWeaver dependencyPropertyWeaver = new DependencyPropertyWeaver(ModuleDefinition);
			AttachedPropertyWeaver attachedPropertyWeaver = new AttachedPropertyWeaver(ModuleDefinition);

			dependencyPropertyWeaver.Execute();
			attachedPropertyWeaver.Execute();
		}

		public override IEnumerable<string> GetAssembliesForScanning()
		{
			yield break;
		}

		public override bool ShouldCleanReference => true;
	}
}