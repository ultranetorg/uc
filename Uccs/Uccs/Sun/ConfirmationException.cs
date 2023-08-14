using System;

namespace Uccs.Net
{
	public class ConnectionFailedException : Exception
	{
		public ConnectionFailedException(string m)
		{
		}

		public ConnectionFailedException(string m, Exception ex) : base(m, ex)
		{
		}
	}
}
