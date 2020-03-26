using CppAst;

namespace Generator
{
    public static class CppPrimitiveTypeExt
    {
        public static string GetOdinType(this CppPrimitiveType self)
        {
            switch (self.Kind)
            {
                case CppPrimitiveKind.Bool:
                    return "bool";
                case CppPrimitiveKind.Char:
                    return "byte";
                case CppPrimitiveKind.Double:
                    return "f64";
                case CppPrimitiveKind.Float:
                    return "f32";
                case CppPrimitiveKind.Int:
                    return "i32";
                case CppPrimitiveKind.LongDouble:
                    throw new System.NotImplementedException();
                case CppPrimitiveKind.LongLong:
                    return "i64";
                case CppPrimitiveKind.Short:
                    return "i16";
                case CppPrimitiveKind.UnsignedChar:
                    return "u8";
                case CppPrimitiveKind.UnsignedInt:
                    return "u32";
                case CppPrimitiveKind.UnsignedLongLong:
                    return "u64";
                case CppPrimitiveKind.UnsignedShort:
                    return "u16";
                case CppPrimitiveKind.Void:
                    return "void";
                case CppPrimitiveKind.WChar:
                    return "bool";
            }
            throw new System.NotImplementedException();
        }
    }
}