using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Tests
{
	internal static class Xon
	{
		public static void Basic()
		{
			var x = new XonDocument(@"a b="""" c=""2 3 4""");
		}
	}
}
