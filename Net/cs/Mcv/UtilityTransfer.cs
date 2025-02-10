namespace Uccs.Net;

public class UtilityTransfer : Operation
{
	public AccountAddress	To;
	public long				BDAmount;
	public long				ECThis;
	public long				ECNext;
	public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(ECThis > 0 ? ECThis + " EC" : null), 
																						  (ECNext > 0 ? ECNext + " EC" : null), 
																						  (BDAmount > 0 ? BDAmount + " BD" : null)}.Where(i => i != null))} -> {To}";
	public override bool	IsValid(Mcv mcv) => BDAmount > 0 || ECThis > 0 || ECNext > 0;

	public UtilityTransfer()
	{
	}

	public UtilityTransfer(AccountAddress to, long ecthis, long ecnext, long bd)
	{
		if(to == null)
			throw new RequirementException("Destination account is null or invalid");

		To			= to;
		ECThis		= ecthis;
		ECNext		= ecnext;
		BDAmount	= bd;
	}

	public override void ReadConfirmed(BinaryReader r)
	{
		To			= r.ReadAccount();
		ECThis		= r.Read7BitEncodedInt64();
		ECNext		= r.Read7BitEncodedInt64();
		BDAmount	= r.Read7BitEncodedInt64();
	}

	public override void WriteConfirmed(BinaryWriter w)
	{
		w.Write(To);
		w.Write7BitEncodedInt64(ECThis);
		w.Write7BitEncodedInt64(ECNext);
		w.Write7BitEncodedInt64(BDAmount);
	}

	public override void Execute(Mcv chain, Round round)
	{
		EC[] d = null;

		if(Signer.Address != chain.Net.God || round.Id > Mcv.LastGenesisRound)
		{
			Signer.EC -= ECThis;
			Signer.ECNext -= ECNext;
			Signer.BDBalance -= BDAmount;
		}

		var to = Affect(round, To);

		to.EC += ECThis;
		to.ECNext += ECNext;
		to.BDBalance += BDAmount;
	}
}
