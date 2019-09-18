using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Bindables.Fody;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Mono.Cecil;
using NUnit.Framework;

// ReSharper disable PossibleNullReferenceException

namespace Bindables.Test
{
	public class Weaver
	{
		public static Assembly Weave(string code)
		{
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

			string[] assemblyPaths =
			{
				typeof(Exception).Assembly.Location,
				typeof(DependencyObject).Assembly.Location,
				typeof(DependencyPropertyAttribute).Assembly.Location,
				typeof(FrameworkPropertyMetadataOptions).Assembly.Location,
				typeof(Console).GetTypeInfo().Assembly.Location,
				Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")
			};

			PortableExecutableReference[] references = assemblyPaths.Select(x => MetadataReference.CreateFromFile(x)).ToArray();

			CSharpCompilation compilation = CSharpCompilation.Create(
				Guid.NewGuid() + ".dll",
				new[] { syntaxTree },
				references,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			using (MemoryStream stream = new MemoryStream())
			{
				EmitResult result = compilation.Emit(stream);

				if (!result.Success)
				{
					Console.Error.WriteLine("Compilation failed!");

					IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
						diagnostic.IsWarningAsError ||
						diagnostic.Severity == DiagnosticSeverity.Error);

					foreach (Diagnostic diagnostic in failures)
					{
						Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
					}

					Assert.That(result.Success);
				}

				stream.Seek(0, SeekOrigin.Begin);

				using (MemoryStream conversionStream = new MemoryStream())
				using (ModuleDefinition module = ModuleDefinition.ReadModule(stream, new ReaderParameters { InMemory = true }))
				{

					ModuleWeaver weavingTask = new ModuleWeaver
					{
						ModuleDefinition = module
					};

					weavingTask.Execute();
					module.Write(conversionStream);
					conversionStream.Seek(0, SeekOrigin.Begin);

					using (FileStream fileStream = File.Create($"{Guid.NewGuid()}.dll"))
					{
						conversionStream.CopyToAsync(fileStream);
						conversionStream.Seek(0, SeekOrigin.Begin);
					}

					return Assembly.Load(conversionStream.ToArray());
				}
			}
		}
	}
}