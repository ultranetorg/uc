using System;
using System.Runtime.Serialization;

namespace Uccs.Net
{
	[Serializable]
	internal class ContinueException : Exception
	{
		public ContinueException()
		{
		}

		public ContinueException(string message) : base(message)
		{
		}

		public ContinueException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}