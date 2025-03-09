namespace Uccs.Rdn;

public enum DomainChange : byte
{
	None, Renew, Transfer, ChangePolicy
}

public class DomainUpdation : UpdateOperation
{
	public new EntityId			Id {get; set;}
	public DomainChange			Change  {get; set;}
	public override string		Description => $"{Id} {Change}={Value}";
	
	public DomainUpdation()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Enum.IsDefined(Change) || Change == DomainChange.None) 
			return false;
		
		if(	(Change == DomainChange.Renew) && 
			(Byte < Mcv.EntityRentYearsMin || Byte > Mcv.EntityRentYearsMax))
			return false;

		if((Change == DomainChange.ChangePolicy) && (!Enum.IsDefined((DomainChildPolicy)Value) || (DomainChildPolicy)Value == DomainChildPolicy.None))
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Change	= reader.ReadEnum<DomainChange>();
		
		Value = Change	switch
						{
							DomainChange.Renew			=> reader.ReadByte(),
							DomainChange.Transfer		=> reader.Read<EntityId>(),
							DomainChange.ChangePolicy	=> reader.ReadEnum<DomainChildPolicy>(),
							_							=> throw new IntegrityException()
						};
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case DomainChange.Renew :			writer.Write(Byte); break;;
			case DomainChange.Transfer :		writer.Write(EntityId); break;;
			case DomainChange.ChangePolicy :	writer.WriteEnum((DomainChildPolicy)Value); break;;
			default								: throw new IntegrityException();
		}
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
			switch(Change)
			{
				case DomainChange.Renew :
				{	
					if(!Domain.CanRegister(e.Address, e, round.ConsensusTime, Signer))
					{
						Error = NotAvailable;
						return;
					}

					e = round.AffectDomain(e.Address);

					PayForName(e.Address, Byte);
					Prolong(round, Signer, e, Time.FromYears(Byte));

					break;
				}

				case DomainChange.Transfer:
				{
					if(!Domain.IsOwner(e, Signer, round.ConsensusTime))
					{
						Error = Denied;
						return;
					}

					if(!RequireAccount(round, EntityId, out var o))
						return;

					e = round.AffectDomain(e.Address);
					e.Owner = EntityId;

					break;
				}
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

			if(Change == DomainChange.Renew)
			{
				if(!Domain.CanRenew(e, Signer, round.ConsensusTime))
				{
					Error = NotAvailable;
					return;
				}

				e = round.AffectDomain(e.Address);

				PayForName(new string(' ', Domain.NameLengthMax), Byte);
				Prolong(round, Signer, e, Time.FromYears(Byte));
			}

			if(Change == DomainChange.ChangePolicy)
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
				e.ParentPolicy = (DomainChildPolicy)Value;
			}

			if(Change == DomainChange.Transfer)
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

				if(!RequireAccount(round, EntityId, out var o))
					return;

				e = round.AffectDomain(e.Address);
				e.Owner	= EntityId;
			}
		}
	}
}
