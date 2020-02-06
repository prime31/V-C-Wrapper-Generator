using System;
using System.IO;
using System.Linq;
using CppAst;

namespace Generator
{
	public static class Debug
	{
		static bool _printComments;

		public static void Dump(this CppCompilation comp, bool printComments = false)
		{
			var typeMap = new TypedefMap(comp.Typedefs);
			_printComments = printComments;

			if (comp.Diagnostics.Messages.Count > 0)
				Console.WriteLine("------ Messages ------");
			foreach (var message in comp.Diagnostics.Messages)
			{
				Console.WriteLine(message);
			}

			if (comp.Macros.Count > 0)
				Console.WriteLine("\n------ Macros ------");
			foreach (var macro in comp.Macros)
			{
				Console.WriteLine(macro);
			}

			if (comp.Typedefs.Count > 0)
				Console.WriteLine("\n------ Typedefs ------");
			foreach (var typedef in comp.Typedefs)
			{
				PrintComment(typedef.Comment);
				Console.WriteLine(typedef);
			}

			if (comp.Enums.Count > 0)
				Console.WriteLine("\n------ Enums ------");
			foreach (var @enum in comp.Enums)
			{
				PrintComment(@enum.Comment);
				Console.WriteLine($"enum {typeMap.GetOrNot(@enum.Name)}");
				Console.WriteLine($"\tType: {@enum.IntegerType}");

				foreach (var t in @enum.Items)
					Console.WriteLine($"\t{t}");

				if (comp.Enums.Last() != @enum)
					Console.WriteLine();
			}

			if (comp.Functions.Count > 0)
				Console.WriteLine("\n------ Functions ------");
			foreach (var cppFunction in comp.Functions)
			{
				PrintComment(cppFunction.Comment);
				Console.WriteLine(cppFunction);
			}

			if (comp.Classes.Count > 0)
				Console.WriteLine("\n------ Structs ------");
			foreach (var cppClass in comp.Classes)
			{
				if (cppClass.ClassKind != CppClassKind.Struct)
				{
					Console.WriteLine($"Error: found a non-struct type! {cppClass.ClassKind} - {cppClass.Name}");
				}
				PrintComment(cppClass.Comment);
				Console.WriteLine($"struct {cppClass.Name}");
				foreach (var field in cppClass.Fields)
				{
					if (field.Type.TypeKind == CppTypeKind.Array)
						Console.WriteLine($"\t-- array --");

					if (field.Type.TypeKind == CppTypeKind.StructOrClass && field.Parent is CppClass parent)
					{
						Console.WriteLine($"\t{parent.Name} {field.Name}");
					}
					else
					{
						var typeName = field.Type.ToString();
						if (field.Type.TypeKind == CppTypeKind.Typedef)
						{
							var t = field.Type as CppTypedef;
							typeName = t.Name;
						}

						Console.WriteLine($"\t{typeName} {field.Name}");
					}
				}

				if (comp.Classes.Last() != cppClass)
					Console.WriteLine();
			}
		}

		static string FilenameFromSpan(CppSourceSpan cppSpan)
		{
			var span = cppSpan.ToString();
			span = span.Substring(0, span.IndexOf("("));
			return Path.GetFileName(span);
		}

		static void PrintComment(CppComment comment)
		{
			if (!_printComments || comment == null)
				return;

			if (!string.IsNullOrEmpty(comment.ToString()))
				Console.WriteLine($"// {comment}");
		}
	}
}