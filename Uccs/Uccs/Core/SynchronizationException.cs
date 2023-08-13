using System;

namespace Uccs.Net
{
	public class SynchronizationException : Exception
	{
		public SynchronizationException()
		{
		}

		public SynchronizationException(string m)
		{
		}

		public SynchronizationException(string m, Exception ex) : base(m, ex)
		{
		}
	}
}
