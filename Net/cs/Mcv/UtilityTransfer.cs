namespace Uccs.Net;

public class UtilityTransfer : Operation
{
	public AccountAddress	To;
	public long				Spacetime;
	public long				Energy;
	public long				EnergyNext;
	public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(Energy > 0 ? Energy + " EC" : null), 
																						  (EnergyNext > 0 ? EnergyNext + " EC" : null), 
																						  (Spacetime > 0 ? Spacetime + " BD" : null)}.Where(i => i != null))} -> {To}";
	public override bool	IsValid(Mcv mcv) => Spacetime > 0 || Energy > 0 || EnergyNext > 0;

	public UtilityTransfer()
	{
	}

	public UtilityTransfer(AccountAddress to, long energy, long energynext, long spacetime)
	{
		if(to == null)
			throw new RequirementException("Destination account is null or invalid");

		To			= to;
		Energy		= energy;
		EnergyNext	= energynext;
		Spacetime	= spacetime;
	}

	public override void ReadConfirmed(BinaryReader r)
	{
		To			= r.ReadAccount();
		Energy		= r.Read7BitEncodedInt64();
		EnergyNext	= r.Read7BitEncodedInt64();
		Spacetime	= r.Read7BitEncodedInt64();
	}

	public override void WriteConfirmed(BinaryWriter w)
	{
		w.Write(To);
		w.Write7BitEncodedInt64(Energy);
		w.Write7BitEncodedInt64(EnergyNext);
		w.Write7BitEncodedInt64(Spacetime);
	}

	public override void Execute(Mcv chain, Round round)
	{
		if(Signer.Address != chain.Net.God || round.Id > Mcv.LastGenesisRound)
		{
			Signer.Energy		-= Energy;
			Signer.EnergyNext	-= EnergyNext;
			Signer.Spacetime	-= Spacetime;
		}

		var to = round.AffectAccount(To, Signer);

		to.Energy		+= Energy;
		to.EnergyNext	+= EnergyNext;
		to.Spacetime	+= Spacetime;
	}
}
