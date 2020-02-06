using System.Collections.Generic;
using CppAst;

namespace Generator
{
	public class TypedefMap
	{
		public Dictionary<string, string> Map = new Dictionary<string, string>();

		public TypedefMap(CppContainerList<CppTypedef> types)
		{
			foreach (var t in types)
			{
				Add(t.ElementType.GetDisplayName(), t.Name);
				Add(t.Name, t.ElementType.GetDisplayName());

				if (t.IsPrimitiveType())
					V.AddTypeConversion(t.Name, t.ElementTypeAsPrimitive().GetVType());
			}
		}

		public void Add(string from, string to) => Map[from] = to;

		public bool Has(string t) => Map.ContainsKey(t);

		public bool TryGet(string t, out string newType) => Map.TryGetValue(t, out newType);

		public string GetOrNot(string t) => Has(t) ? Map[t] : t;
	}
}