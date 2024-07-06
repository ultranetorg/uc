using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Uccs
{
	public class StringEnum<T>
	{
		public static string[] Names => All.Select(i => i.Name).ToArray();
		public static string[] Values => All.Select(i => i.GetValue(null) as string).ToArray();

		static IEnumerable<FieldInfo> All => typeof(T).GetFields().Where(i => i.Attributes.HasFlag(System.Reflection.FieldAttributes.Literal));

		public static string ValueByName(string name)
		{
			return All.First(i => i.Name == name).GetValue(null) as string;
		}

		public static string NameByValue(string val)
		{
			return All.First(i => i.GetValue(null) as string == val).Name;
		}
	}
}
