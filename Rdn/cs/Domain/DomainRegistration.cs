﻿namespace Uccs.Rdn;

public class DomainRegistration : RdnOperation
{
	public string				Address {get; set;}
	public byte					Years {get; set;}
	public AccountAddress		Owner  {get; set;}
	public DomainChildPolicy	Policy {get; set;}

	public override string		Description => $"{Address} for {Years} years";
	
	public DomainRegistration ()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Domain.Valid(Address))
			return false;
		
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Address	= reader.ReadUtf8();
		Years = reader.ReadByte();

		if(Domain.IsChild(Address))
		{
			Owner = reader.Read<AccountAddress>();
			Policy	= reader.ReadEnum<DomainChildPolicy>();
		}
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.WriteUtf8(Address);
		writer.Write(Years);

		if(Domain.IsChild(Address))
		{
			writer.Write(Owner);
			writer.WriteEnum(Policy);
		}
	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		var e = mcv.Domains.Find(Address, round.Id);

		if(Domain.IsRoot(Address))
		{
			if(!Domain.CanRegister(Address, e, round.ConsensusTime, Signer))
			{
				Error = NotAvailable;
				return;
			}

			e = round.AffectDomain(Address);
			
			PayForName(Address, Years);
			Prolong(round, Signer, e, Time.FromYears(Years));

			///if(Domain.IsWeb(e.Address)) /// distribite winner bid, one time
			///	Transaction.BYReturned += e.LastBid;
							
			e.Owner			= Signer.Id;
			e.LastWinner	= null;
			e.LastBid		= 0;
			e.LastBidTime	= Time.Empty;
			e.FirstBidTime	= Time.Empty;
		}
		else
		{
			var p = mcv.Domains.Find(Domain.GetParent(Address), round.Id);

			if(e != null)
			{
				Error = AlreadyExists;
				return;
			}

			if(!Domain.IsOwner(p, Signer, round.ConsensusTime))
			{
				Error = Denied;
				return;
			}

			if(Policy < DomainChildPolicy.FullOwnership || DomainChildPolicy.FullFreedom < Policy)
			{
				Error = NotAvailable;
				return;
			}

			e = round.AffectDomain(Address);
			
			var start = e.Expiration < round.ConsensusTime ? round.ConsensusTime : e.Expiration;

			e.Owner			= mcv.Accounts.Find(Owner, round.Id).Id;
			e.ParentPolicy	= Policy;
			e.Expiration	= start + Time.FromYears(Years);

			PayForName(new string(' ', Domain.NameLengthMax), Years);
		}
	}
}
