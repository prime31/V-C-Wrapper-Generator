using System;
using System.Linq;
using CppAst;
using System.Text.Json;
using System.IO;

namespace Generator
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Contains("-h"))
			{
				PrintHelp();
			}
			else if (args.Contains("-c"))
			{
				if (args.Length != 2)
				{
					Console.WriteLine("Invalid arguments. The -c option requires a filename");
					PrintHelp();
					return;
				}
				var index = Array.IndexOf(args, "-c");
				var file = args[index + 1];

				var options = new JsonSerializerOptions
				{
					WriteIndented = true
				};
				var json = JsonSerializer.Serialize(ConfigExamples.GetSDLConfig(), options);
				File.WriteAllText(file, json);
			}
			else if (args.Contains("-g"))
			{
				Console.WriteLine("Invalid arguments. The -g option requires a filename");
				if (args.Length != 2)
				{
					PrintHelp();
					return;
				}
				var index = Array.IndexOf(args, "-g");
				var file = args[index + 1];

				var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(file));
				Run(config);
			}
			else
			{
				// PrintHelp();

				// Run(ConfigExamples.GetKincConfig());
				// Run(ConfigExamples.GetPhyFSConfig());
				// Run(ConfigExamples.GetSDLConfig());
				// Run(ConfigExamples.GetLuaConfig());
				// Run(ConfigExamples.GetFlecsConfig());
				// Run(ConfigExamples.GetSoloudConfig());
				// Run(ConfigExamples.GetFNA3DConfig());
				Run(ConfigExamples.GetImGuiConfig());
			}
		}

		private static void PrintHelp()
		{
			Console.WriteLine("VGenerator Help");
			Console.WriteLine("Standard usage pattern is to first use the '-c' option to create a template generator JSON file.");
			Console.WriteLine("The generator JSON file will have SDL2 data in it as an example so that you can see what the params look like.");
			Console.WriteLine("Fill in the details of the JSON file then use the '-g' option to generate the V bindings.");
			Console.WriteLine("\nUsage:");
			Console.WriteLine("\tWrite an empty generator configuration json file:");
			Console.WriteLine("\tVGenerator -c FILENAME");
			Console.WriteLine("\n\tGenerate V bindings from a generator configuration json file:");
			Console.WriteLine("\tVGenerator -g CONFIG_FILENAME");
		}

		static void Run(Config config)
		{
			var compilation = CppParser.ParseFiles(config.GetFiles(), config.ToParserOptions());
			OdinGenerator.Generate(config, compilation);
			if (compilation.Diagnostics.HasErrors)
				compilation.Dump();
		}

	}
}
