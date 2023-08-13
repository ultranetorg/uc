using System;
using System.Runtime.Serialization;

namespace Uccs.Net
{
	[Serializable]
	internal class ContinueSearchException : Exception
	{
		public ContinueSearchException()
		{
		}

		public ContinueSearchException(string message) : base(message)
		{
		}

		public ContinueSearchException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}