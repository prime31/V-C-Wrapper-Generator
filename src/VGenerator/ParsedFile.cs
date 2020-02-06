using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CppAst;

namespace Generator
{
	public class ParsedFile
	{
		public string Filename;
		public string Folder;
		public List<CppTypedef> Typedefs = new List<CppTypedef>();
		public List<CppEnum> Enums = new List<CppEnum>();
		public List<CppFunction> Functions = new List<CppFunction>();
		public List<ParsedFunction> ParsedFunctions = new List<ParsedFunction>();
		public List<CppClass> Structs = new List<CppClass>();

		public ParsedFile(string filename, string folder)
		{
			Filename = filename;
			Folder = folder ?? "";
		}


		public static List<ParsedFile> ParseIntoFiles(CppCompilation comp, Config config)
		{
			var spans = new HashSet<CppSourceSpan>();
			var files = new List<ParsedFile>();

			foreach (var typedef in comp.Typedefs)
			{
				var file = GetOrCreateFile(files, typedef.Span, false);
				file.Typedefs.Add(typedef);
				spans.Add(typedef.Span);
			}

			foreach (var e in comp.Enums)
			{
				if (string.IsNullOrEmpty(e.Name))
				{
					Console.WriteLine($"Found nameless enum with {e.Items.Count} items! [{e.Span.FilePath()}]");
					continue;
				}

				var file = GetOrCreateFile(files, e.Span, false);
				file.Enums.Add(e);
				spans.Add(e.Span);
			}

			foreach (var function in comp.Functions)
			{
				if (config.IsFunctionExcluded(function.Name))
					continue;

				var file = GetOrCreateFile(files, function.Span, false);
				file.Functions.Add(function);
				file.ParsedFunctions.Add(ParsedFunctionFromCppFunction(function, config));
				spans.Add(function.Span);
			}

			foreach (var klass in comp.Classes)
			{
				var file = GetOrCreateFile(files, klass.Span, false);
				file.Structs.Add(klass);
				spans.Add(klass.Span);
			}

			if (config.CopyHeadersToDstDir)
			{
				foreach (var span in spans)
					span.CopyTo(config);
			}

			return files.Where(f => !config.IsFileExcluded(f)).ToList();
		}

		static ParsedFile GetOrCreateFile(List<ParsedFile> files, CppSourceSpan span, bool singleFileExport)
		{
			if (singleFileExport)
			{
				if (files.Count == 0)
					files.Add(new ParsedFile(null, null));
				return files[0];
			}

			var filename = span.FilenameNoExtension();
			var folder = span.Folder();

			var file = files.Where(f => f.Filename == filename && f.Folder == folder).FirstOrDefault();
			if (file == null)
			{
				file = new ParsedFile(filename, folder);
				files.Add(file);
			}
			return file;
		}

		/// <summary>
		/// Deals with turning the CppFunction types into V types, function name to snake case, params
		/// to snake case and any other data transformations we will need for the V functions
		/// </summary>
		static ParsedFunction ParsedFunctionFromCppFunction(CppFunction cFunc, Config config)
		{
			var f = new ParsedFunction
			{
				Name = cFunc.Name,
				VName = V.ToSnakeCase(config.StripFunctionPrefix(cFunc.Name))
			};

			// hack to fix ghetto forced 'init' module function
			if (f.VName == "init")
				f.VName = config.ModuleName + "_" + f.VName;

			if (cFunc.ReturnType.GetDisplayName() != "void")
			{
				f.RetType = cFunc.ReturnType.GetDisplayName();
				f.VRetType = V.GetVType(cFunc.ReturnType);
			}

			foreach (var param in cFunc.Parameters)
			{
				var p = new ParsedParameter
				{
					Name = param.Name.EscapeReserved(),
					VName = V.ToSnakeCase(config.StripFunctionPrefix(param.Name)).EscapeReserved(),
					Type = param.Type.GetDisplayName(),
					VType = V.GetVType(param.Type)
				};
				f.Parameters.Add(p);
			}

			return f;
		}
	}

	public class ParsedFunction
	{
		public string Name, VName;
		public string RetType, VRetType;
		public List<ParsedParameter> Parameters = new List<ParsedParameter>();
	}

	public class ParsedParameter
	{
		public string Name, VName;
		public string Type, VType;
	}
}