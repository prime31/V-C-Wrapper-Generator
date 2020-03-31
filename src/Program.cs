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
				var json = JsonSerializer.Serialize(GetSDLConfig(), options);
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

				// Run(GetKincConfig());
				// Run(GetPhyFSConfig());
				// Run(GetSDLConfig());
				// Run(GetLuaConfig());
				// Run(GetFlecsConfig());
				// Run(GetSoloudConfig());
				Run(GetFNA3DConfig());
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
			// compilation.Dump();
		}

		#region Example Configs

		static Config GetFNA3DConfig()
		{
			return new Config
			{
				DstDir = "~/Desktop/FNA3D_Odin",
				SrcDir = "~/Desktop/FNA3D",
				BaseSourceFolder = "",
				ModuleName = "fna",
				NativeLibName = "FNA3D",
				SkipLinkNameFunctionsThatContain = new string[] {},
				EnumWordDictionary = new string[] {"equal", "counter", "clockwise", "face", "short", "vector", "coordinate", "indices",
					"weight", "factor", "size", "left", "right", "contents", "buffer", "list", "strip", "byte", "single",
					"bgra", "blendable", "only", "source", "color", "destination", "saturation", "alpha", "blend", "subtract",
					"2d", "3d"},
				FunctionWordDictionary = new string[] {"2D", "3D", "YUV", "DXT1", "S3TC"},
				ExcludeFunctionsThatContain = new string[] {},
				StripPrefixFromFunctionNames = new string[] {"FNA3D_"},
				CTypeToOdinType = {},
				Defines = new string[] {},
				IncludeFolders = new string[] {
					"include"
				},
				Files = new string[] {
					"FNA3D.h"
				},
				ExcludedFiles = new string[] {},
				ExcludedFromOdinWrapperFiles = new string[] {}
			};
		}

		static Config GetSoloudConfig()
		{
			return new Config
			{
				DstDir = "~/Desktop/OdinSoloud",
				SrcDir = "~/Desktop",
				BaseSourceFolder = "",
				ModuleName = "soloud",
				ExcludeFunctionsThatContain = new string[] {},
				StripPrefixFromFunctionNames = new string[] {"Soloud_"},
				CTypeToOdinType = {},
				Defines = new string[] {},
				IncludeFolders = new string[] {
					""
				},
				Files = new string[] {
					"Soloud_c.h"
				},
				ExcludedFiles = new string[] {},
				ExcludedFromOdinWrapperFiles = new string[] {}
			};
		}

		static Config GetLuaConfig()
		{
			return new Config
			{
				DstDir = "~/Desktop/lua",
				SrcDir = "~/Desktop/lua-5.3.5/src",
				BaseSourceFolder = "src",
				ModuleName = "lua",
				ExcludeFunctionsThatContain = new string[] {},
				StripPrefixFromFunctionNames = new string[] {},
				CTypeToOdinType = {
					{"__sFILE", "rawptr"}
				},
				Defines = new string[] {},
				IncludeFolders = new string[] { "src" },
				Files = new string[] {
					"lua.h", "lualib.h", "lauxlib.h"
				},
				ExcludedFiles = new string[] {},
				ExcludedFromOdinWrapperFiles = new string[] {}
			};
		}

		static Config GetFlecsConfig()
		{
			return new Config
			{
				DstDir = "~/Desktop/SDL2",
				SrcDir = "~/Desktop/flecs/include",
				BaseSourceFolder = "src",
				ModuleName = "flecs",
				ExcludeFunctionsThatContain = new string[] {},
				StripPrefixFromFunctionNames = new string[] {"ecs_", "_ecs_", "Ecs"},
				CTypeToOdinType = {},
				Defines = new string[] { "FLECS_NO_CPP" },
				IncludeFolders = new string[] {
					"",
					"flecs",
					"flecs/util"
				},
				Files = new string[] {
					"flecs.h"
				},
				ExcludedFiles = new string[] {},
				ExcludedFromOdinWrapperFiles = new string[] {}
			};
		}

		static Config GetSDLConfig()
		{
			return new Config
			{
				DstDir = "~/Desktop/SDL2/sdl",
				SrcDir = "/usr/local/include/SDL2",
				BaseSourceFolder = "src",
				ModuleName = "sdl",
				ExcludeFunctionsThatContain = new string[] {},
				StripPrefixFromFunctionNames = new string[] { "SDL_"},
				CTypeToOdinType = {
					{"__sFILE", "rawptr"}
				},
				Defines = new string[] {},
				IncludeFolders = new string[] {},
				Files = new string[] {
					"SDL.h"
				},
				ExcludedFiles = new string[] {
					"SDL_main", "SDL_audio", "SDL_assert", "SDL_atomic", "SDL_mutex",
					"SDL_thread", "SDL_gesture", "SDL_sensor", "SDL_power", "SDL_render", "SDL_shape",
					"SDL_endian", "SDL_cpuinfo", "SDL_loadso", "SDL_system"
				},
				ExcludedFromOdinWrapperFiles = new string[] {
					"SDL_stdinc"
				}
			};
		}

		static Config GetPhyFSConfig()
		{
			return new Config
			{
				DstDir = "~/Desktop/PhysFSGen",
				SrcDir = "~/Desktop/physfs/src",
				BaseSourceFolder = "src",
				ModuleName = "c",
				ExcludeFunctionsThatContain = new string[] {},
				StripPrefixFromFunctionNames = new string[] { "PHYSFS_"},
				CTypeToOdinType = {},
				TargetSystem = "darwin",
				Defines = new string[] {},
				IncludeFolders = new string[] {},
				Files = new string[] {
					"physfs.h"
				}
			};
		}

		static Config GetKincConfig()
		{
			return new Config
			{
				DstDir = "~/Desktop/KincGen",
				SrcDir = "~/Desktop/kha/Shader-Kinc/Kinc",
				BaseSourceFolder = "kinc",
				ModuleName = "c",
				ExcludeFunctionsThatContain = new string[] { "_internal_" },
				StripPrefixFromFunctionNames = new string[] { "kinc_g4_", "kinc_g5_", "kinc_", "LZ4_" },
				CTypeToOdinType = {
					{"kinc_ticks_t", "u64"}
				},
				TargetSystem = "darwin",
				Defines = new string[] {
					"KORE_MACOS", "KORE_METAL", "KORE_POSIX", "KORE_G1", "KORE_G2", "KORE_G3", "KORE_G4",
					"KORE_G5", "KORE_G4ONG5", "KORE_MACOS", "KORE_METAL", "KORE_POSIX", "KORE_A1", "KORE_A2",
					"KORE_A3", "KORE_NO_MAIN"
				},
				IncludeFolders = new string[] {
					"Sources",
					"Backends/System/Apple/Sources",
					"Backends/System/macOS/Sources",
					"Backends/System/POSIX/Sources",
					"Backends/Graphics5/Metal/Sources",
					"Backends/Graphics4/G4onG5/Sources",
					"Backends/Audio3/A3onA2/Sources"
				},
				Files = new string[] {
					"kinc/graphics1/graphics.h",
					"kinc/graphics4/constantlocation.h",
					"kinc/graphics4/graphics.h",
					"kinc/graphics4/indexbuffer.h",
					"kinc/graphics4/rendertarget.h",
					"kinc/graphics4/shader.h",
					"kinc/graphics4/texture.h",
					"kinc/graphics4/texturearray.h",
					"kinc/graphics4/textureunit.h",
					"kinc/graphics5/commandlist.h",
					"kinc/graphics5/constantbuffer.h",
					"kinc/input/gamepad.h",
					"kinc/input/keyboard.h",
					"kinc/input/mouse.h",
					"kinc/input/surface.h",
					"kinc/io/filereader.h",
					"kinc/io/filewriter.h",
					"kinc/math/random.h",
					"kinc/system.h",
					"kinc/window.h"
				}
			};
		}

		#endregion
	}
}
