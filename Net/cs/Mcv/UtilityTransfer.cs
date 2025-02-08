namespace Uccs.Net;

public class UtilityTransfer : Operation
{
	public AccountAddress	To;
	public long				BYAmount;
	public long				ECThis;
	public long				ECNext;
	public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(ECThis > 0 ? ECThis + " EC" : null), 
																						  (ECNext > 0 ? ECNext + " EC" : null), 
																						  (BYAmount > 0 ? BYAmount + " BY" : null)}.Where(i => i != null))} -> {To}";
	public override bool	IsValid(Mcv mcv) => BYAmount > 0 || ECThis > 0 || ECNext > 0;

	public UtilityTransfer()
	{
	}

	public UtilityTransfer(AccountAddress to, long ecthis, long ecnext, long by)
	{
		if(to == null)
			throw new RequirementException("Destination account is null or invalid");

		To			= to;
		ECThis		= ecthis;
		ECNext		= ecnext;
		BYAmount	= by;
	}

	public override void ReadConfirmed(BinaryReader r)
	{
		To			= r.ReadAccount();
		ECThis		= r.Read7BitEncodedInt64();
		ECNext		= r.Read7BitEncodedInt64();
		BYAmount	= r.Read7BitEncodedInt64();
	}

	public override void WriteConfirmed(BinaryWriter w)
	{
		w.Write(To);
		w.Write7BitEncodedInt64(ECThis);
		w.Write7BitEncodedInt64(ECNext);
		w.Write7BitEncodedInt64(BYAmount);
	}

	public override void Execute(Mcv chain, Round round)
	{
		EC[] d = null;

		if(Signer.Address != chain.Net.God || round.Id > Mcv.LastGenesisRound)
		{
			Signer.ECThis -= ECThis;
			Signer.ECNext -= ECNext;
			Signer.BYBalance -= BYAmount;
		}

		var to = Affect(round, To);

		to.ECThis += ECThis;
		to.ECNext += ECNext;
		to.BYBalance += BYAmount;
	}
}
