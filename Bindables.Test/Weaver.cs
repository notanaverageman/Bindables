using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Bindables.Fody;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using Mono.Cecil;
using NUnit.Framework;

// ReSharper disable PossibleNullReferenceException

namespace Bindables.Test
{
	public class Weaver
	{
		public static Assembly Weave(string code)
		{
			string assemblyName = Guid.NewGuid() + ".dll";

			CompilerParameters parameters = new CompilerParameters
			{
				GenerateExecutable = false,
				OutputAssembly = assemblyName
			};

			parameters.ReferencedAssemblies.Add("System.dll");
			parameters.ReferencedAssemblies.Add(typeof(DependencyObject).Assembly.Location);
			parameters.ReferencedAssemblies.Add(typeof(DependencyPropertyAttribute).Assembly.Location);
			parameters.ReferencedAssemblies.Add(typeof(FrameworkPropertyMetadataOptions).Assembly.Location);
			parameters.ReferencedAssemblies.Add(typeof(Color).Assembly.Location);

			CSharpCodeProvider codeProvider = CreateCodeProvider();

			CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(parameters, code);

			Assert.IsEmpty(compilerResults.Errors);

			using (MemoryStream stream = new MemoryStream())
			{
				ModuleDefinition module = ModuleDefinition.ReadModule(assemblyName);
				File.Delete(assemblyName);

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

		private static CSharpCodeProvider CreateCodeProvider()
		{
			CSharpCodeProvider codeProvider = new CSharpCodeProvider();

			// Fix the roslyn folder path.
			object settings =
				codeProvider.GetType()
					.GetField("_compilerSettings", BindingFlags.Instance | BindingFlags.NonPublic)
					.GetValue(codeProvider);

			string path =
				settings.GetType().GetField("_compilerFullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(settings) as
					string;

			settings.GetType()
				.GetField("_compilerFullPath", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(settings, path.Replace(@"bin\roslyn\", @"roslyn\"));
			return codeProvider;
		}
	}
}