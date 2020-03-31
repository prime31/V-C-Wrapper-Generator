using System;
using System.IO;
using System.Linq;
using CppAst;

namespace Generator
{
	public static class OdinGenerator
	{
		public static void Generate(Config config, CppCompilation comp)
		{
			var typeMap = new TypedefMap(comp.Typedefs);

			Odin.AddTypeConversions(config.CTypeToOdinType);

			foreach (var t in comp.Typedefs)
			{
				var name = t.Name;
				var odinName = Odin.ToAdaCase(name);
				Odin.AddTypeConversion(name, odinName);

				Console.WriteLine($"AddTypeConversion: {odinName} :: struct {{}}");
			}

			// add conversions for any types in the lib
			foreach (var s in comp.Classes)
			{
				var mappedName = typeMap.GetOrNot(s.Name);
				if (mappedName != s.Name)
					Odin.AddTypeConversion(s.Name, mappedName);
				Odin.AddTypeConversion(typeMap.GetOrNot(s.Name), Odin.GetOdinStructName(config.StripFunctionPrefix(s.Name)));
			}

			// enums will all be replaced by our Odin enums
			foreach (var e in comp.Enums)
			{
				var mappedName = typeMap.GetOrNot(e.Name);
				if (mappedName != e.Name)
					Odin.AddTypeConversion(mappedName, Odin.GetOdinEnumName(config.StripFunctionPrefix(e.Name)));
				Odin.AddTypeConversion(e.Name, Odin.GetOdinEnumName(config.StripFunctionPrefix(e.Name)));
			}

			Directory.CreateDirectory(config.DstDir);

			var procWriter = new StreamWriter(File.Open(Path.Combine(config.DstDir, "procs.odin"), FileMode.Create));
			procWriter.WriteLine($"package {config.ModuleName}");
			procWriter.WriteLine();
			WriteForeignBoilerplate(config, procWriter);

			var typeWriter = new StreamWriter(File.Open(Path.Combine(config.DstDir, "types.odin"), FileMode.Create));
			typeWriter.WriteLine($"package {config.ModuleName}");

			var parsedFiles = ParsedFile.ParseIntoFiles(comp, config);
			foreach (var file in parsedFiles)
				WriteFiles(config, file, procWriter, typeWriter);
			procWriter.WriteLine("}");

			procWriter.Dispose();
			typeWriter.Dispose();
		}

		static void WriteForeignBoilerplate(Config config, StreamWriter procWriter)
		{
			var libName = config.ModuleName + "_lib";
			procWriter.WriteLine($"when ODIN_OS == \"windows\" do foreign import {libName} \"native/{config.NativeLibName}.lib\";");
			procWriter.WriteLine($"when ODIN_OS == \"linux\" do foreign import {libName} \"native/lib{config.NativeLibName}.so\";");
			procWriter.WriteLine($"when ODIN_OS == \"darwin\" do foreign import {libName} \"native/lib{config.NativeLibName}.dylib\";");
			procWriter.WriteLine();
			procWriter.WriteLine($"foreign {libName} {{");
		}

		static void WriteFiles(Config config, ParsedFile file, StreamWriter procWriter, StreamWriter typeWriter)
		{
			var module = config.ModuleName;

			if (file.ParsedFunctions.Count > 1)
				procWriter.WriteLine($"\t// {file.Folder}/{file.Filename}");

			if (file.Structs.Count + file.Enums.Count > 0 )
			{
				typeWriter.WriteLine();
				typeWriter.WriteLine($"// {file.Folder}/{file.Filename}");
				typeWriter.WriteLine();
			}

			// write out our types and our C declarations first
			foreach (var e in file.Enums)
				WriteEnum(typeWriter, e, config);

			foreach (var s in file.Structs)
				WriteStruct(typeWriter, s, config);

			foreach (var f in file.ParsedFunctions)
				WriteProc(procWriter, f);
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
				enumItemNames = enumItemNames.Select(n => prefix.Length > 0 ? n.Replace(prefix, "") : n).ToArray();
			}

			writer.WriteLine($"{Odin.GetOdinEnumName(config.StripFunctionPrefix(e.Name))} :: enum i32 {{");
			for (var i = 0; i < e.Items.Count; i++)
			{
				var item = e.Items[i];
				writer.Write($"\t{Odin.GetOdinEnumItemName(enumItemNames[i], config)}");

				if (hasValue)
					writer.Write($" = {item.Value}");
				if (i != e.Items.Count - 1)
					writer.Write(",");
				writer.WriteLine();
			}

			writer.WriteLine("}");
			writer.WriteLine();
		}

		static void WriteStruct(StreamWriter writer, CppClass s, Config config)
		{
			var structName = Odin.GetOdinStructName(config.StripFunctionPrefix(s.Name));
			if (s.Fields.Count == 0)
			{
				writer.WriteLine($"{structName} :: struct {{}}");
				writer.WriteLine();
				return;
			}

			writer.WriteLine($"{structName} :: struct {{");
			foreach (var f in s.Fields)
			{
				var type = Odin.GetOdinType(f.Type);
				var name = Odin.GetCFieldName(f.Name);
				writer.Write($"\t{name}: {type}");

				if (s.Fields.Last() != f)
					writer.WriteLine(",");
				else
					writer.WriteLine();
			}

			writer.WriteLine("}");
			writer.WriteLine();
		}

		static void WriteProc(StreamWriter writer, ParsedFunction func)
		{
			if (!func.SkipLinkNameAttribute)
				writer.WriteLine($"\t@(link_name = \"{func.Name}\")");

			writer.Write($"\t{func.OdinName} :: proc(");
			foreach (var p in func.Parameters)
			{
				writer.Write($"{p.Name}: {p.OdinType}");
				if (func.Parameters.Last() != p)
					writer.Write(", ");
			}
			writer.Write(")");

			if (func.RetType != null)
				writer.Write($" -> {func.OdinRetType}");
			writer.Write(" ---;");
			writer.WriteLine();
			writer.WriteLine();
		}

		static void WriteOdinFunction(StreamWriter writer, ParsedFunction func)
		{
			// first, V function def
			writer.WriteLine("[inline]");
			writer.Write($"pub fn {func.OdinName}(");
			foreach (var p in func.Parameters)
			{
				// special case for byteptr which we convert to string for the function def
				if (p.OdinType == "byteptr")
					writer.Write($"{p.OdinName} string");
				else
					writer.Write($"{p.OdinName} {p.OdinType}");

				if (func.Parameters.Last() != p)
					writer.Write(", ");
			}
			writer.Write(")");

			if (func.OdinRetType != null)
			{
				if (func.OdinRetType == "byteptr")
					writer.WriteLine(" string {");
				else
					writer.WriteLine($" {func.OdinRetType} {{");
			}
			else
			{
				writer.WriteLine(" {");
			}

			// now the function body calling the C function
			writer.Write("\t");
			if (func.OdinRetType != null)
			{
				writer.Write("return ");

				// special case for byteptr, which we cast to string
				if (func.OdinRetType == "byteptr")
					writer.Write("string(");
			}

			writer.Write($"C.{func.Name}(");
			foreach (var p in func.Parameters)
			{
				// special case for byteptr which was converted to string above
				if (p.OdinType == "byteptr")
					writer.Write($"{p.OdinName}.str");
				else
					writer.Write($"{p.OdinName}");

				if (func.Parameters.Last() != p)
					writer.Write(", ");
			}

			// close the string cast if we are returning a byteptr cast to string
			if (func.OdinRetType == "byteptr")
				writer.Write(")");

			writer.WriteLine(")");
			writer.WriteLine("}");

			writer.WriteLine();
		}
	}
}