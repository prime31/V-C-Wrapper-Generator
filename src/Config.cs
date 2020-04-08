using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CppAst;

namespace Generator
{
	public class Config
	{
		public string SrcDir {get; set;}
		public string DstDir {get; set;}
		public string ModuleName {get; set;}

		/// <summary>
		/// when converting enums these words will be searched for an used to break up the name when Ada casing
		/// </summary>
		public string[] EnumWordDictionary {get; set;} = new string[] {};

		/// <summary>
		/// when converting enums these words will be searched for an used to break up the name when Ada casing
		/// </summary>
		public string[] FunctionWordDictionary {get; set;} = new string[] {};

		/// <summary>
		/// the name of the lib name. For windows it will be NAME.lib, mac libNAME.dylib and linux libNAME.so
		/// </summary>
		public string NativeLibName {get; set;}
		public bool CopyHeadersToDstDir {get; set;} = false;

		/// <summary>
		/// if a function name contains any of the strings present here it will be excluded from
		/// code generation.
		/// </summary>
		public string[] ExcludeFunctionsThatContain {get; set;} = new string[] {};

		/// <summary>
		/// if a function name contains any of the strings present here it will not have a link_name and the
		/// raw C function name will be used
		/// </summary>
		public string[] SkipLinkNameFunctionsThatContain {get; set;} = new string[] {};

		/// <summary>
		/// if a function name starts with any prefix present, it will be stripped before writing the
		/// Odin function. Note that this is the C function prefix.
		/// </summary>
		public string[] StripPrefixFromFunctionNames {get; set;} = new string[] {};

		/// <summary>
		/// if true, if there is a common prefix for all the enum values it will be stripped when
		/// generating the Odin items
		/// </summary>
		public bool StripEnumItemCommonPrefix {get; set;} = true;

		/// <summary>
		/// used when UseHeaderFolder is true to determine if a file should be placed in the module
		/// root folder. If the folder the file is in is BaseSourceFolder it will be placed in the
		/// root folder.
		/// </summary>
		public string BaseSourceFolder {get; set;}

		/// <summary>
		/// if true, the folder the header is in will be used for the generated Odin file
		/// </summary>
		public bool UseHeaderFolder {get; set;} = true;

		/// <summary>
		/// custom map of C types to Odin types. Most default C types will be handled automatically.
		/// </summary>
		public Dictionary<string, string> CTypeToOdinType {get; set;} = new Dictionary<string, string>();

		/// <summary>
		/// All the header files that should be parsed and converted.
		/// </summary>
		public string[] Files {get; set;}

		/// <summary>
		/// All the header files that should be excluded from conversion. If a file should be declared in the c declaration
		/// but not wrapped it should be added to ExcludedFromOdinWrapperFiles
		/// </summary>
		public string[] ExcludedFiles {get; set;}

		/// <summary>
		/// All the header files that should be excluded from the Odin wrapper
		/// </summary>
		public string[] ExcludedFromOdinWrapperFiles {get; set;}

		/// <summary>
		/// List of the defines.
		/// </summary>
		public string[] Defines {get; set;} = new string[] {};

		/// <summary>
		/// List of the include folders.
		/// </summary>
		public string[] IncludeFolders {get; set;} = new string[] {};

		/// <summary>
		/// List of the system include folders.
		/// </summary>
		public string[] SystemIncludeFolders {get; set;} = new string[] {};

		/// <summary>
		/// List of the additional arguments passed directly to the C++ Clang compiler.
		/// </summary>
		public string[] AdditionalArguments {get; set;} = new string[] {};

		/// <summary>
		/// Gets or sets a boolean indicating whether un-named enum/struct referenced by a typedef will be
		/// renamed directly to the typedef name. Default is <c>true</c>
		/// </summary>
		public bool AutoSquashTypedef {get; set;} = true;

		/// <summary>
		/// Controls whether the codebase should be parsed as C or C++
		/// </summary>
		public bool ParseAsCpp {get; set;} = true;

		public bool ParseComments {get; set;} = false;

		/// <summary>
		/// System Clang target. Default is "darwin"
		/// </summary>
		public string TargetSystem {get; set;} = "darwin";

		public CppParserOptions ToParserOptions()
		{
			Validate();
			AddSystemIncludes();

			var opts = new CppParserOptions();
			opts.Defines.AddRange(Defines);
			opts.IncludeFolders.AddRange(ToAbsolutePaths(IncludeFolders));
			opts.SystemIncludeFolders.AddRange(SystemIncludeFolders);
			opts.AdditionalArguments.AddRange(AdditionalArguments);
			opts.AutoSquashTypedef = AutoSquashTypedef;
			opts.TargetSystem = TargetSystem;
			opts.ParseComments = ParseComments;

			return opts;
		}

		void Validate()
		{
			// resolve paths
			var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			if (!Path.IsPathRooted(DstDir))
				DstDir = DstDir.Replace("~", homeFolder);

			if (!Path.IsPathRooted(SrcDir))
				SrcDir = SrcDir.Replace("~", homeFolder);

			if (string.IsNullOrEmpty(ModuleName))
				throw new ArgumentException(nameof(ModuleName));
			if (string.IsNullOrEmpty(SrcDir))
				throw new ArgumentException(nameof(SrcDir));
			if (string.IsNullOrEmpty(DstDir))
				throw new ArgumentException(nameof(DstDir));

			if (Files.Length == 0)
				throw new ArgumentException(nameof(Files));

			// we want longest first so we match them before shorter strings
			Array.Sort(EnumWordDictionary, (x, y) => y.Length.CompareTo(x.Length));
			Array.Sort(StripPrefixFromFunctionNames, (x, y) => y.Length.CompareTo(x.Length));

			// exlude filenames dont need extensions
			ExcludedFiles = ExcludedFiles.Select(f => f.Replace(".h", "")).ToArray();
			ExcludedFromOdinWrapperFiles = ExcludedFromOdinWrapperFiles.Select(f => f.Replace(".h", "")).ToArray();
		}

		void AddSystemIncludes()
		{
			if (TargetSystem == "darwin")
			{
				SystemIncludeFolders = SystemIncludeFolders.Union(new string[] {
					"/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/include",
					"/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk/usr/include",
					"/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/clang/11.0.3/include"
				}).Distinct().ToArray();
			}
		}

		/// <summary>
		/// Retuns all Files as absolute paths. If a file path is relative, it will try to resolve its location using
		/// the IncludeFolders and the SrcDir.
		/// </summary>
		public List<string> GetFiles()
			=> Files.Select(p => Path.IsPathRooted(p) ? p : IncludedFileToAbsPath(p)).ToList();

		/// <summary>
		/// If any of paths are relative, returns the path appended to SrcDir.
		/// </summary>
		string[] ToAbsolutePaths(string[] paths)
			=> paths.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(SrcDir, p)).ToArray();

		/// <summary>
		///	Attempts to resolve an included file path by checking all the IncludeFolder and SrcDir to get an absolute
		/// path to the file.
		/// </summary>
		string IncludedFileToAbsPath(string path)
		{
			var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			foreach (var incPath in ToAbsolutePaths(IncludeFolders))
			{
				var tmp = Path.Combine(incPath, path);
				if (File.Exists(tmp))
					return tmp;

				if (!Path.IsPathRooted(tmp))
				{
					tmp = tmp.Replace("~", homeFolder);
					if (File.Exists(tmp))
						return tmp;
				}
			}

			if (!Path.IsPathRooted(SrcDir))
				SrcDir = SrcDir.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.Personal));

			var newPath = Path.Combine(SrcDir, path);
			if (!File.Exists(newPath))
				throw new FileNotFoundException($"Could not find file {path} from the Files array. Maybe your {nameof(IncludeFolders)} are not correct?");
			return newPath;
		}
	}

	public static class ConfigExt
	{
		public static bool IsFunctionExcluded(this Config config, string function)
		{
			return config.ExcludeFunctionsThatContain.Where(exclude => function.Contains(exclude)).Any();
		}

		public static bool IsFunctionNotLinked(this Config config, string function)
		{
			return config.SkipLinkNameFunctionsThatContain.Where(exclude => function.Contains(exclude)).Any();
		}

		public static string StripFunctionPrefix(this Config config, string function)
		{
			var prefixes = config.StripPrefixFromFunctionNames.Where(p => function.StartsWith(p));
			if (prefixes.Count() > 0)
			{
				var longestPrefix = prefixes.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
				return longestPrefix.Length > 0 ? function.Replace(longestPrefix, "") : function;
			}
			return function;
		}

		public static bool IsFileExcluded(this Config config, ParsedFile file)
		{
			return config.ExcludedFiles.Contains(file.Filename)
				|| config.ExcludedFiles.Contains(Path.Combine(file.Folder, file.Filename));
		}

		public static bool IsFileExcludedFromVWrapper(this Config config, ParsedFile file)
		{
			return config.ExcludedFromOdinWrapperFiles.Contains(file.Filename)
				|| config.ExcludedFromOdinWrapperFiles.Contains(Path.Combine(file.Folder, file.Filename));
		}
	}
}