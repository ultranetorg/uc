using System;

namespace Uccs.Net
{
	public class ConfirmationException : Exception
	{
		Round Round;

		public ConfirmationException(string m, Round r) : base(m)
		{
			Round = r;
		}
	}
}
