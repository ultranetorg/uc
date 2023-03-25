using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Sun.CLI
{
	public class SyntaxException : Exception
	{
		public SyntaxException(string msg) : base(msg)
		{
		}
	}
}
