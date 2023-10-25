using System;
using System.Diagnostics;

namespace Uccs.Net
{
	public class ConfirmationException : Exception
	{
		Round Round;

		public ConfirmationException(Round r)
		{
			Round = r;

			//#if DEBUG
			//Debugger.Break();
			//#endif
		}
	}
}
