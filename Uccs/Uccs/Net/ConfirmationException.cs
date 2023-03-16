using System;

namespace UC.Net
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
