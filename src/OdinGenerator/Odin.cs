using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CppAst;

namespace Generator
{
	public static class Odin
	{
		static Dictionary<string, string> cTypeToOdinType = new Dictionary<string, string>
		{
			{"bool", "bool"},
			{"void*", "rawptr"},
			{"void**", "&rawptr"},
			{"char", "byte"},
			{"char*", "cstring"},
			{"wchar", "u16"},
			{"int8_t", "i8"},
			{"short", "i16"},
			{"int16_t", "i16"},
			{"size_t", "uint"},
			{"int", "i32"},
			{"int*", "&i32"},
			{"int32_t", "i32"},
			{"int64_t", "i64"},
			{"long", "i64"},
			{"float", "f32"},
			{"double", "f64"},
			{"uint8_t*", "&byte"},
			{"uint8_t", "byte"},
			{"uint16_t", "u16"},
			{"unsigned short", "u16"},
			{"uint32_t", "u32"},
			{"unsigned int", "u32"},
			{"unsigned long", "u64"},
			{"uint64_t", "u64"},
			{"unsigned char", "byte"},
			{"const char *", "cstring"},
			{"const char*", "cstring"},
			{"const void *", "rawptr"},
			{"const void*", "rawptr"},
			{"unsigned char*", "cstring"},
			{"unsigned char *", "cstring"},
			{"const char**", "&rawptr"}
		};

		static string[] reserved = new[] { /*"map",*/ "string", "return", "or", "none", "type", "select", "false", "true", "module" };

		public static void AddTypeConversions(Dictionary<string, string> types)
		{
			foreach (var t in types)
			{
				if (!cTypeToOdinType.ContainsKey(t.Value))
					cTypeToOdinType[t.Key] = t.Value;
			}
		}

		public static void AddTypeConversion(string type) => cTypeToOdinType[type] = type;

		public static void AddTypeConversion(string type, string toType) => cTypeToOdinType[type] = toType;

		public static string GetOdinType(CppType cppType)
		{
			// unwrap any const vars
			if (cppType.TypeKind == CppTypeKind.Qualified && cppType is CppQualifiedType cppQualType)
			{
				if (cppQualType.Qualifier == CppTypeQualifier.Const)
					return GetOdinType(cppQualType.ElementType);
			}

			if (cppType is CppClass cppClass && cppClass.ClassKind == CppClassKind.Union)
			{
				Console.WriteLine($"Found union we can't handle! [{cppType.Span}]");
				return "rawptr";
			}

			if (cppType.TypeKind == CppTypeKind.Enum || cppType.TypeKind == CppTypeKind.Primitive)
				return GetOdinType(cppType.GetDisplayName());

			if (cppType.TypeKind == CppTypeKind.Typedef && cppType is CppTypedef typeDefType)
			{
				if (typeDefType.IsPrimitiveType())
					return typeDefType.ElementTypeAsPrimitive().GetVType();
				else
					return GetOdinType(typeDefType.ElementType);
			}

			if (cppType.TypeKind == CppTypeKind.Pointer)
			{
				var cppPtrType = cppType as CppPointerType;

				// special V types
				if (cppPtrType.GetDisplayName() == "const char*" || cppPtrType.GetDisplayName() == "char*")
					return "byteptr";

				if (cppPtrType.GetDisplayName() == "const void*" || cppPtrType.GetDisplayName() == "void*")
					return "rawptr";

				// double pointer check
				if (cppPtrType.ElementType.TypeKind == CppTypeKind.Pointer)
				{
					if (cppPtrType.ElementType.TypeKind == CppTypeKind.Pointer)
						return $"&rawptr /* {cppPtrType.GetDisplayName()} */";
					return $"^{GetOdinType(cppPtrType.ElementType)} /* {cppPtrType.GetDisplayName()} */";
				}

				// unwrap any const vars
				if (cppPtrType.ElementType.TypeKind == CppTypeKind.Qualified && cppPtrType.ElementType is CppQualifiedType qualType)
				{
					if (qualType.Qualifier == CppTypeQualifier.Const)
					{
						if (qualType.ElementType is CppPrimitiveType qualPrimType && qualPrimType.Kind == CppPrimitiveKind.Void)
							return $"rawptr";
						return "^" + GetOdinType(qualType.ElementType);
					}
				}

				// function pointers
				if (cppPtrType.ElementType.TypeKind == CppTypeKind.Function)
				{
					var funcType = cppPtrType.ElementType as CppFunctionType;
					if (funcType.Parameters.Count == 1 && funcType.Parameters[0].Type is CppPointerType cppPtrPtrType && cppPtrPtrType.ElementType.TypeKind == CppTypeKind.Function)
						funcType = cppPtrPtrType.ElementType as CppFunctionType;

					// HACK: for some reason, there can occassionally be function pointer args inside a function pointer argument
					// for some reason. We just peel away the top function pointer and use that as the full type to fix the issue.
					foreach (var p in funcType.Parameters)
					{
						if (p.Type is CppPointerType pType && pType.ElementType.TypeKind == CppTypeKind.Function)
							funcType = pType.ElementType as CppFunctionType;
					}

					string GetReturnType()
					{
						// void return
						if (funcType.ReturnType is CppPrimitiveType cppPrimType && cppPrimType.Kind == CppPrimitiveKind.Void)
							return null;
						return "-> " + GetOdinType(funcType.ReturnType);
					};

					// easy case: no parameters
					if (funcType.Parameters.Count == 0)
						return $"proc() {GetReturnType()}".TrimEnd();

					var sb = new StringBuilder();
					sb.Append("proc(");
					foreach (var p in funcType.Parameters)
					{
						var paramType = GetOdinType(p.Type);
						if (paramType.Contains("proc"))
						{
							// TODO: typedef the function param
							var typeDef = $"type Fn{Odin.ToPascalCase(p.Name)} {paramType}";
						}
						sb.Append(paramType);

						if (funcType.Parameters.Last() != p)
							sb.Append(", ");
					}
					sb.Append(")");

					var ret = GetReturnType();
					if (ret != null)
						sb.Append($" {ret}");

					// check for a function pointer that has a function as a param.
					var definition = sb.ToString();
					if (definition.LastIndexOf("proc") > 2)
						return $"rawptr /* {definition} */";
					return sb.ToString();
				}
				else if (cppPtrType.ElementType.TypeKind == CppTypeKind.Typedef)
				{
					// functions dont get passed with '&' so we have to see if this Typedef has a function in its lineage
					if (cppPtrType.ElementType is CppTypedef td && td.IsFunctionType())
						return GetOdinType(cppPtrType.ElementType);
					return "^" + GetOdinType(cppPtrType.ElementType);
				}

				return "^" + GetOdinType(cppPtrType.ElementType.GetDisplayName());
			} // end Pointer

			if (cppType.TypeKind == CppTypeKind.Array)
			{
				var arrType = cppType as CppArrayType;
				if (arrType.ElementType is CppClass arrParamClass)
				{
					if (arrParamClass.Name.Contains("va_"))
					{
						Console.WriteLine($"Found unhandled vararg param! [{cppType}]");
						return "rawptr /* ...rawptr */";
					}
				}
				var eleType = GetOdinType(arrType.ElementType);
				if (arrType.Size > 0)
					return $"[{arrType.Size}]{eleType}";
				return $"[]{eleType}";
			}

			return GetOdinType(cppType.GetDisplayName());
		}

		public static string GetOdinType(string type)
		{
			if (cTypeToOdinType.TryGetValue(type, out var vType))
				return vType;

			Console.WriteLine($"no conversion found for {type}");
			return type;
		}

		public static string GetOdinEnumName(string name)
		{
			if (name.EndsWith("_t"))
				name = name.Substring(0, name.Length - 2);

			return ToAdaCase(name);
		}

		public static string GetOdinStructName(string name)
		{
			if (name.EndsWith("_t"))
				name = name.Substring(0, name.Length - 2);

			return ToAdaCase(name);
		}

		public static string GetCFieldName(string name)
		{
			if (reserved.Contains(name))
				return "@" + name;
			return name;
		}

		public static string GetOdinEnumItemName(string name)
		{
			if (name.Contains('_'))
				return name.MakeSafeEnumItem();

			if (name.IsUpper())
				name = ToAdaCase(name).MakeSafeEnumItem();

			return ToAdaCase(name).MakeSafeEnumItem();
		}

		/// <summary>
		/// escapes a reserved name for use as a parameter.
		/// </summary>
		public static string EscapeReserved(this string name)
		{
			if (name == "none")
				return "non";

			if (name == "type")
				return "typ";

			if (name == "false")
				return "no";

			if (name == "true")
				return "yes";

			if (name == "return")
				return "ret";

			if (name == "select")
				return "sel";

			if (name == "module")
				return "mod";

			if (reserved.Contains(name))
				throw new System.Exception($"need escape for {name}");
			return name;
		}

		/// <summary>
		/// escapes a reserved field name with a @ for correct functionality with C names
		/// </summary>
		public static string EscapeReservedField(string name)
		{
			if (reserved.Contains(name))
				return $"@{name}";
			return name;
		}

		public static string ToSnakeCase(string name)
		{
			if (name.IsLower())
				return name;

			if (name.Contains("_"))
				return name.ToLower();

			name = string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
			return EscapeReserved(name);
		}

		public static string ToAdaCase(string original)
		{
			var str = ToPascalCase(original);
			str = string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
			return str;
		}

		public static string ToPascalCase(string original)
		{
			var invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
			var whiteSpace = new Regex(@"(?<=\s)");
			var startsWithLowerCaseChar = new Regex("^[a-z]");
			var firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
			var lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
			var upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

			// replace white spaces with undescore, then replace all invalid chars with empty string
			var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(original, "_"), string.Empty)
				// split by underscores
				.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
				// set first letter to uppercase
				.Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
				// replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
				.Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
				// set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
				.Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
				// lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
				.Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

			return string.Concat(pascalCase);
		}

		public static bool IsUpper(this string value)
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (char.IsLower(value[i]))
					return false;
			}
			return true;
		}

		public static bool IsLower(this string value)
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (char.IsUpper(value[i]) && value[i] != '_')
					return false;
			}
			return true;
		}

		public static string MakeSafeEnumItem(this string name)
		{
			if (char.IsDigit(name[0]))
				return "_" + name;

			return EscapeReserved(name);
		}
	}
}