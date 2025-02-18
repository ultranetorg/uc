namespace Uccs.Rdn;

public enum DomainAction
{
	None, Renew, Transfer, ChangePolicy
}

public class DomainUpdation : RdnOperation
{
	public new EntityId			Id {get; set;}
	public DomainAction			Action  {get; set;}
	public byte					Years {get; set;}
	public AccountAddress		Owner  {get; set;}
	public DomainChildPolicy	Policy {get; set;}

	//public bool					Exclusive => Domain.IsWeb(Address); 
	public override string		Description => $"{Id} for {Years} years";
	
	public DomainUpdation()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Enum.IsDefined(Action) || Action == DomainAction.None) 
			return false;
		
		if(	(Action == DomainAction.Renew) && 
			(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
			return false;

		if((Action == DomainAction.ChangePolicy) && (!Enum.IsDefined(Policy) || Policy == DomainChildPolicy.None))
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Action	= (DomainAction)reader.ReadByte();

		if(Action == DomainAction.Renew)
			Years = reader.ReadByte();

		if(Action == DomainAction.Transfer)
			Owner = reader.Read<AccountAddress>();
		
		if(Action == DomainAction.ChangePolicy)
			Policy	= (DomainChildPolicy)reader.ReadByte();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write((byte)Action);

		if(Action == DomainAction.Renew)
			writer.Write(Years);

		if(Action == DomainAction.Transfer)
			writer.Write(Owner);

		if(Action == DomainAction.ChangePolicy)
			writer.Write((byte)Policy);

	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		var e = mcv.Domains.Find(Id, round.Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}			

		if(Domain.IsRoot(e.Address))
		{
			if(Action == DomainAction.Renew)
			{	
				if(!Domain.CanRegister(e.Address, e, round.ConsensusTime, Signer))
				{
					Error = NotAvailable;
					return;
				}

				e = round.AffectDomain(e.Address);

				PayForName(e.Address, Years);
				Prolong(round, Signer, e, (short)Time.FromYears(Years).Days);
			}

			if(Action == DomainAction.Transfer)
			{
				if(!Domain.IsOwner(e, Signer, round.ConsensusTime))
				{
					Error = Denied;
					return;
				}

				e = round.AffectDomain(e.Address);
				e.Owner	= round.AffectAccount(Owner, Signer).Id;
			}
		} 
		else
		{
			var p = mcv.Domains.Find(Domain.GetParent(e.Address), round.Id);

			if(p == null)
			{
				Error = NotFound;
				return;
			}

			if(Action == DomainAction.Renew)
			{
				if(!Domain.CanRenew(e, Signer, round.ConsensusTime))
				{
					Error = NotAvailable;
					return;
				}

				e = round.AffectDomain(e.Address);

				PayForName(new string(' ', Domain.NameLengthMax), Years);
				Prolong(round, Signer, e, (short)Time.FromYears(Years).Days);
			}

			if(Action == DomainAction.ChangePolicy)
			{
				if(e == null)
				{
					Error = NotFound;
					return;
				}

				if(!Domain.IsOwner(p, Signer, round.ConsensusTime))
				{
					Error = Denied;
					return;
				}

				if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !Domain.IsExpired(e, round.ConsensusTime))
				{
					Error = NotAvailable;
					return;
				}

				e = round.AffectDomain(e.Address);
				e.ParentPolicy = Policy;
			}

			if(Action == DomainAction.Transfer)
			{
				if(e.ParentPolicy == DomainChildPolicy.FullOwnership && !Domain.IsOwner(p, Signer, round.ConsensusTime))
				{
					Error = NotAvailable;
					return;
				}

				if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !Domain.IsOwner(e, Signer, round.ConsensusTime) && 
																	  !(Domain.IsOwner(p, Signer, round.ConsensusTime) && Domain.IsExpired(e, round.ConsensusTime)))
				{
					Error = NotAvailable;
					return;
				}

				e = round.AffectDomain(e.Address);
				e.Owner	= round.AffectAccount(Owner, Signer).Id;
			}
		}
	}
}
