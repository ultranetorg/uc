using System;
using System.Diagnostics;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public class ConfirmationException : Exception
	{
		public Round Round { get; protected set; }

		public ConfirmationException(Round r, byte[] summary) : base(typeof(ConfirmationException).Name + " - " + 
																$"Confirmation failed at {r.Id} round, summary={r.Summary.ToHex()}, Msummary={summary.ToHex()}")
		{
			Round = r;

			//#if DEBUG
			//Debugger.Break();
			//#endif
		}
	}
}
