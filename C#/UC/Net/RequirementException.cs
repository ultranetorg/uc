using System;
using System.Collections.Generic;
using System.Text;

namespace UC.Net
{
	public class RequirementException : Exception
	{
		public RequirementException(string m) : base(m)
		{
		}

		public RequirementException(string m, Exception inner) : base(m, inner)
		{
		}
	}
}
