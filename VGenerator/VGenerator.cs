using System;
using System.IO;
using System.Linq;
using CppAst;

namespace Generator
{
	public static class VGenerator
	{
		public static void Generate(Config config, CppCompilation comp)
		{
			var typeMap = new TypedefMap(comp.Typedefs);

			V.AddTypeConversions(config.CTypeToVType);

			// add conversions for any types in the lib
			foreach (var s in comp.Classes)
			{
				var mappedName = typeMap.GetOrNot(s.Name);
				if (mappedName != s.Name)
					V.AddTypeConversion(s.Name, mappedName);
				V.AddTypeConversion(typeMap.GetOrNot(s.Name));
			}

			// enums will all be replaced by our V enums
			foreach (var e in comp.Enums)
			{
				var mappedName = typeMap.GetOrNot(e.Name);
				if (mappedName != e.Name)
					V.AddTypeConversion(mappedName, V.GetVEnumName(config.StripFunctionPrefix(e.Name)));
				V.AddTypeConversion(e.Name, V.GetVEnumName(config.StripFunctionPrefix(e.Name)));
			}

			Directory.CreateDirectory(config.DstDir);

			StreamWriter writer = null;
			if (config.SingleVFileExport)
			{
				writer = new StreamWriter(File.Open(Path.Combine(config.DstDir, config.CDeclarationFileName), FileMode.Create));
				writer.WriteLine($"module {config.ModuleName}");
			}

			var parsedFiles = ParsedFile.ParseIntoFiles(comp, config);
			foreach (var file in parsedFiles)
				WriteFile(config, file, writer);
			writer?.Dispose();

			// now we write the V wrapper
			writer = new StreamWriter(File.Open(Path.Combine(config.DstDir, config.VWrapperFileName), FileMode.Create));
			writer.WriteLine($"module {config.ModuleName}\n");
			foreach (var file in parsedFiles.Where(f => !config.IsFileExcludedFromVWrapper(f)))
			{
				foreach (var func in file.ParsedFunctions)
					WriteVFunction(writer, func);
			}
			writer.Dispose();
		}

		static void WriteFile(Config config, ParsedFile file, StreamWriter writer)
		{
			var module = config.ModuleName;
			if (writer == null && config.UseHeaderFolder && config.BaseSourceFolder != file.Folder)
			{
				module = file.Folder;
				var dst = Path.Combine(config.DstDir, file.Folder);
				Directory.CreateDirectory(dst);
				writer = new StreamWriter(File.Open(Path.Combine(dst, file.Filename + ".v"), FileMode.Create));
			}
			else if(writer == null)
			{
				writer = new StreamWriter(File.Open(Path.Combine(config.DstDir, file.Filename + ".v"), FileMode.Create));
			}

			if (config.SingleVFileExport)
			{
				writer.WriteLine();
				writer.WriteLine($"// {file.Folder}/{file.Filename}");
			}
			else
			{
				writer.WriteLine($"module {module}");
			}
			writer.WriteLine();

			// write out our types and our C declarations first
			foreach (var e in file.Enums)
				WriteEnum(writer, e, config);

			foreach (var s in file.Structs)
				WriteStruct(writer, s);

			foreach (var f in file.ParsedFunctions)
				WriteCFunction(writer, f);

			if (!config.SingleVFileExport)
				writer.Dispose();
		}

		static void WriteEnum(StreamWriter writer, CppEnum e, Config config)
		{
			var hasValue = e.Items.Where(i => i.ValueExpression != null && !string.IsNullOrEmpty(i.ValueExpression.ToString())).Any();

			var enumItemNames = e.Items.Select(i => i.Name).ToArray();
			if (config.StripEnumItemCommonPrefix && e.Items.Count > 1)
			{
				string CommonPrefix(string str, params string[] more)
				{
					var prefixLength = str
									  .TakeWhile((c, i) => more.All(s => i < s.Length && s[i] == c))
									  .Count();

					return str.Substring(0, prefixLength);
				}

				var prefix = CommonPrefix(e.Items[0].Name, e.Items.Select(i => i.Name).Skip(1).ToArray());
				enumItemNames = enumItemNames.Select(n => n.Replace(prefix, "")).ToArray();
			}

			writer.WriteLine($"pub enum {V.GetVEnumName(config.StripFunctionPrefix(e.Name))} {{");
			for (var i = 0; i < e.Items.Count; i++)
			{
				var item = e.Items[i];
				writer.Write($"\t{V.GetVEnumItemName(enumItemNames[i])}");

				if (hasValue)
					writer.Write($" = {item.Value}");
				writer.WriteLine();
			}

			writer.WriteLine("}");
			writer.WriteLine();
		}

		static void WriteStruct(StreamWriter writer, CppClass s)
		{
			writer.WriteLine($"pub struct {V.GetVType(s.Name)} {{");
			if (s.Fields.Count > 0)
				writer.WriteLine("pub:");

			foreach (var f in s.Fields)
			{
				var type = V.GetVType(f.Type);
				var name = V.GetCFieldName(f.Name);
				writer.WriteLine($"\t{name} {type}");
			}

			writer.WriteLine("}");
			writer.WriteLine();
		}

		static void WriteCFunction(StreamWriter writer, ParsedFunction func)
		{
			writer.Write($"fn C.{func.Name}(");
			foreach (var p in func.Parameters)
			{
				writer.Write($"{p.Name} {p.VType}");
				if (func.Parameters.Last() != p)
					writer.Write(", ");
			}
			writer.Write(")");

			if (func.RetType != null)
				writer.Write($" {func.VRetType}");
			writer.WriteLine();
		}

		static void WriteVFunction(StreamWriter writer, ParsedFunction func)
		{
			// first, V function def
			writer.WriteLine("[inline]");
			writer.Write($"pub fn {func.VName}(");
			foreach (var p in func.Parameters)
			{
				// special case for byteptr which we convert to string for the function def
				if (p.VType == "byteptr")
					writer.Write($"{p.VName} string");
				else
					writer.Write($"{p.VName} {p.VType}");

				if (func.Parameters.Last() != p)
					writer.Write(", ");
			}
			writer.Write(")");

			if (func.VRetType != null)
			{
				if (func.VRetType == "byteptr")
					writer.WriteLine(" string {");
				else
					writer.WriteLine($" {func.VRetType} {{");
			}
			else
			{
				writer.WriteLine(" {");
			}

			// now the function body calling the C function
			writer.Write("\t");
			if (func.VRetType != null)
			{
				writer.Write("return ");

				// special case for byteptr, which we cast to string
				if (func.VRetType == "byteptr")
					writer.Write("string(");
			}

			writer.Write($"C.{func.Name}(");
			foreach (var p in func.Parameters)
			{
				// special case for byteptr which was converted to string above
				if (p.VType == "byteptr")
					writer.Write($"{p.VName}.str");
				else
					writer.Write($"{p.VName}");

				if (func.Parameters.Last() != p)
					writer.Write(", ");
			}

			// close the string cast if we are returning a byteptr cast to string
			if (func.VRetType == "byteptr")
				writer.Write(")");

			writer.WriteLine(")");
			writer.WriteLine("}");

			writer.WriteLine();
		}
	}
}