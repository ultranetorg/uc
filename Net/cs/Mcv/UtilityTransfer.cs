namespace Uccs.Net;

public class UtilityTransfer : Operation
{
	public AccountAddress	To;
	public long				BYAmount;
	public long				ECAmount;
	public Time				ECExpiration;
	public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(ECAmount > 0 ? ECAmount + " EC" : null), 
																						  (BYAmount > 0 ? BYAmount + " BY" : null),
																						  }.Where(i => i != null))} -> {To}";
	public override bool	IsValid(Mcv mcv) => BYAmount > 0 || ECAmount > 0;

	public UtilityTransfer()
	{
	}

	public UtilityTransfer(AccountAddress to, long ec, Time expiration, long by)
	{
		if(to == null)
			throw new RequirementException("Destination account is null or invalid");

		To			= to;
		ECAmount	= ec;
		ECExpiration= expiration;
		BYAmount	= by;
	}

	public override void ReadConfirmed(BinaryReader r)
	{
		To				= r.ReadAccount();
		ECAmount		= r.Read7BitEncodedInt64();
		ECExpiration	= r.Read<Time>();
		BYAmount		= r.Read7BitEncodedInt64();
	}

	public override void WriteConfirmed(BinaryWriter w)
	{
		w.Write(To);
		w.Write7BitEncodedInt64(ECAmount);
		w.Write(ECExpiration);
		w.Write7BitEncodedInt64(BYAmount);
	}

	public override void Execute(Mcv chain, Round round)
	{
		EC[] d = null;

		if(Signer.Address != chain.Net.God || round.Id > Mcv.LastGenesisRound)
		{
			if(ECExpiration != Time.Empty)
			{
				var i = Signer.ECBalance.FindIndex(i => i.Expiration == ECExpiration);
				
				if(i == -1 || Signer.ECBalance[i].Amount < ECAmount)
				{
					Error = NotEnoughEC;
					return;
				}

				Signer.ECBalance[i] = new (ECExpiration, Signer.ECBalance[i].Amount - ECAmount);
			}
			else
			{
				if(Signer.Integrate(round.ConsensusTime) < ECAmount)
				{
					Error = NotEnoughEC;
					return;
				}
				
				d = Signer.ECBalanceDifference(round.ConsensusTime, ECAmount);
				Signer.ECBalanceSubtract(round.ConsensusTime, ECAmount);
			}

			Signer.BYBalance -= BYAmount;
		}

		var to = Affect(round, To);

		if(ECExpiration != Time.Empty)
			to.ECBalanceAdd(new EC(ECExpiration, ECAmount));
		else
			to.ECBalanceAdd(d);

		to.BYBalance += BYAmount;
	}
}
