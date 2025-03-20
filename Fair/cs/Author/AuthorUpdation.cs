namespace Uccs.Fair;

public class AuthorRenewal : FairOperation
{
	public EntityId				AuthorId { get; set; }
	public byte					Years { get; set; }

	public override string		Description => $"{AuthorId}, {Years}";
	
	public AuthorRenewal()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		AuthorId	= reader.Read<EntityId>();
		Years		= reader.ReadByte();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireAuthorAccess(execution, AuthorId, out var a))
			return;
		
		a = execution.AffectAuthor(AuthorId);

		if(!Author.CanRenew(a, Signer, execution.Time))
		{
			Error = NotAvailable;
			return;
		}

		Prolong(execution, Signer, a, Time.FromYears(Years));
	}
}

public class AuthorModerationReward : FairOperation
{
	public EntityId				AuthorId { get; set; }
	public long					Amount { get; set; }

	public override string		Description => $"{AuthorId}, {Amount}";
	
	public AuthorModerationReward()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		AuthorId	= reader.Read<EntityId>();
		Amount		= reader.Read7BitEncodedInt64();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.Write7BitEncodedInt64(Amount);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireAuthorAccess(execution, AuthorId, out var a))
			return;
		
		a = execution.AffectAuthor(AuthorId);

		a.ModerationReward = Amount;
	}
}

public class AuthorOwnerAddition : FairOperation
{
	public EntityId				AuthorId { get; set; }
	public EntityId				Owner { get; set; }

	public override string		Description => $"{AuthorId}, {Owner}";
	
	public AuthorOwnerAddition()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		AuthorId	= reader.Read<EntityId>();
		Owner		= reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.Write(Owner);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireAuthorAccess(execution, AuthorId, out var a))
			return;
		
		a = execution.AffectAuthor(AuthorId);

		if(!RequireAccount(execution, Owner, out var x))
			return;

		if(x.AllocationSponsor != null)
		{
			Error = NotAllowedForFreeAccount;
			return;
		}

		a.Owners = [..a.Owners, x.Id];
	}
}

public class AuthorOwnerRemoval : FairOperation
{
	public EntityId				AuthorId { get; set; }
	public EntityId				Owner { get; set; }

	public override string		Description => $"{AuthorId}, {Owner}";
	
	public AuthorOwnerRemoval()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		AuthorId	= reader.Read<EntityId>();
		Owner		= reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.Write(Owner);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireAuthorAccess(execution, AuthorId, out var a))
			return;
		
		a = execution.AffectAuthor(AuthorId);

		if(a.Owners.Length == 1)
		{
			Error = AtLeastOneOwnerRequired;
			return;
		}

		if(!RequireAccount(execution, Owner, out var x))
			return;

		a.Owners = a.Owners.Remove(x.Id);
	}
}
