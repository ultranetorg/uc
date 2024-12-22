namespace Uccs.Net;

public class ConfirmationException : Exception
{
	public Round Round { get; protected set; }

	public ConfirmationException(Round r, byte[] hashbymajority) : base(typeof(ConfirmationException).Name + " - " + 
																	$"Confirmation failed at {r.Id} round, Hash={r.Hash?.ToHex()}, HashByMajority={hashbymajority?.ToHex()}")
	{
		Round = r;

		//#if DEBUG
		//Debugger.Break();
		//#endif
	}
}
