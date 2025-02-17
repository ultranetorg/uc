namespace Uccs.Net;

public class UtilityTransfer : Operation
{
	public AccountAddress	To;
	public long				ST;
	public long				EC;
	public long				ECNext;
	public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(EC > 0 ? EC + " EC" : null), 
																						  (ECNext > 0 ? ECNext + " EC" : null), 
																						  (ST > 0 ? ST + " BD" : null)}.Where(i => i != null))} -> {To}";
	public override bool	IsValid(Mcv mcv) => ST > 0 || EC > 0 || ECNext > 0;

	public UtilityTransfer()
	{
	}

	public UtilityTransfer(AccountAddress to, long ec, long ecnext, long bd)
	{
		if(to == null)
			throw new RequirementException("Destination account is null or invalid");

		To		= to;
		EC		= ec;
		ECNext	= ecnext;
		ST		= bd;
	}

	public override void ReadConfirmed(BinaryReader r)
	{
		To		= r.ReadAccount();
		EC		= r.Read7BitEncodedInt64();
		ECNext	= r.Read7BitEncodedInt64();
		ST		= r.Read7BitEncodedInt64();
	}

	public override void WriteConfirmed(BinaryWriter w)
	{
		w.Write(To);
		w.Write7BitEncodedInt64(EC);
		w.Write7BitEncodedInt64(ECNext);
		w.Write7BitEncodedInt64(ST);
	}

	public override void Execute(Mcv chain, Round round)
	{
		EC[] d = null;

		if(Signer.Address != chain.Net.God || round.Id > Mcv.LastGenesisRound)
		{
			Signer.Energy		-= EC;
			Signer.EnergyNext	-= ECNext;
			Signer.Spacetime	-= ST;
		}

		var to = round.AffectAccount(To, Signer);

		to.Energy		+= EC;
		to.EnergyNext	+= ECNext;
		to.Spacetime	+= ST;
	}
}
