using System;

namespace UC.Net
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
