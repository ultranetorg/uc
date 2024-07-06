using System;

namespace Uccs
{
	public class SyntaxException : Exception
	{
		public SyntaxException(string msg) : base(msg)
		{
		}
	}
}
