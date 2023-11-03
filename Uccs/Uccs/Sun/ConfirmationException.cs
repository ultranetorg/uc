using System;
using System.Diagnostics;

namespace Uccs.Net
{
	public class ConfirmationException : Exception
	{
		public Round Round { get; protected set; }

		public ConfirmationException(Round r) : base(typeof(ConfirmationException).Name + " - " + $"Confirmation failed at {r.Id} round")
		{
			Round = r;

			//#if DEBUG
			//Debugger.Break();
			//#endif
		}
	}
}
