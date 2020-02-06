using CppAst;

namespace Generator
{
    public static class CppTypedefExt
    {
		public static bool IsPrimitiveType(this CppTypedef typedef)
            => typedef.ElementType.TypeKind == CppTypeKind.Primitive;

        public static CppPrimitiveType ElementTypeAsPrimitive(this CppTypedef typedef)
            => typedef.ElementType as CppPrimitiveType;

        public static bool IsFunctionType(this CppTypedef typedef)
        {
            // search the ElementType hiearchy for CppTypedefs and CppPointers until we finally get to a CppFunction or not
            if (typedef.ElementType is CppTypedef td)
                return td.IsFunctionType();

            if (typedef.ElementType is CppPointerType ptr)
            {
                if (ptr.ElementType.TypeKind == CppTypeKind.Function)
                    return true;
            }

            if (typedef.ElementType.TypeKind == CppTypeKind.Function)
                return true;

            return false;
        }
	}
}