using System;

namespace Uccs.Net
{
	public class ResourceException : Exception
	{
		public ResourceException()
		{
		}

		public ResourceException(string m) : base(m)
		{
		}

		public ResourceException(string m, Exception inner) : base(m, inner)
		{
		}
	}
}
