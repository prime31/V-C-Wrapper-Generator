namespace Generator
{
    public static class ConfigExamples
    {
        public static Config GetImGuiConfig() => new Config
        {
            DstDir = "~/Desktop/cimgui_odin",
            SrcDir = "~/Desktop/cimgui",
            BaseSourceFolder = "",
            ModuleName = "imgui",
            ExcludeFunctionsThatContain = new string[] { },
            StripPrefixFromFunctionNames = new string[] { "ig", "ImGui" },
            FunctionWordDictionary = new string[] { "ImDraw" },
            CTypeToOdinType = { },
            Defines = new string[] { "CIMGUI_DEFINE_ENUMS_AND_STRUCTS" },
            IncludeFolders = new string[] {
                "imgui", "."
            },
            Files = new string[] {
                "cimgui.h"
            },
            ExcludedFiles = new string[] { },
            ExcludedFromOdinWrapperFiles = new string[] { }
        };

        public static Config GetMojoShaderConfig()
		{
			return new Config
			{
				DstDir = "~/Desktop/Mojo_Odin",
				SrcDir = "~/Desktop/MojoShader",
				BaseSourceFolder = "",
				ModuleName = "fna",
				NativeLibName = "FNA3D",
				SkipLinkNameFunctionsThatContain = new string[] {},
				EnumWordDictionary = new string[] {"equal", "clockwise", "face", "short", "vector", "coordinate", "indices",
					"weight", "factor", "size", "left", "right", "contents", "buffer", "list", "strip", "byte", "single",
					"bgra", "blendable", "only", "source", "color", "destination", "saturation", "alpha", "blend", "subtract"},
				ExcludeFunctionsThatContain = new string[] {},
				StripPrefixFromFunctionNames = new string[] {"MOJOSHADER_"},
				CTypeToOdinType = {},
				Defines = new string[] {},
				IncludeFolders = new string[] {
					""
				},
				Files = new string[] {
					"mojoshader.h"
				},
				ExcludedFiles = new string[] {},
				ExcludedFromOdinWrapperFiles = new string[] {}
			};
		}

		public static Config GetFNA3DConfig()
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
				FunctionWordDictionary = new string[] {"2D", "3D", "YUV", "DXT1", "S3TC", "JPG", "PNG"},
				ExcludeFunctionsThatContain = new string[] {},
				StripPrefixFromFunctionNames = new string[] {"FNA3D_", "FNA3D_Image_"},
				CTypeToOdinType = {},
				Defines = new string[] {},
				IncludeFolders = new string[] {
					"include"
				},
				Files = new string[] {
					"FNA3D.h", "FNA3D_Image.h"
				},
				ExcludedFiles = new string[] {},
				ExcludedFromOdinWrapperFiles = new string[] {}
			};
		}

		public static Config GetSoloudConfig()
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

		public static Config GetLuaConfig()
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

		public static Config GetFlecsConfig()
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

		public static Config GetSDLConfig()
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

		public static Config GetPhyFSConfig()
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

		public static Config GetKincConfig()
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

    }
}