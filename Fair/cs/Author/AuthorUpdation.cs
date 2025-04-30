namespace Uccs.Fair;

public class AuthorRenewal : FairOperation
{
	public AutoId				AuthorId { get; set; }
	public byte					Years { get; set; }

	public override string		Explanation => $"{AuthorId}, {Years}";
	
	public AuthorRenewal()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		AuthorId	= reader.Read<AutoId>();
		Years		= reader.ReadByte();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireAuthorAccess(execution, AuthorId, out var a))
			return;
		
		a = execution.Authors.Affect(AuthorId);

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
	public AutoId				AuthorId { get; set; }
	public long					Amount { get; set; }

	public override string		Explanation => $"{AuthorId}, {Amount}";
	
	public AuthorModerationReward()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		AuthorId	= reader.Read<AutoId>();
		Amount		= reader.Read7BitEncodedInt64();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.Write7BitEncodedInt64(Amount);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireAuthorAccess(execution, AuthorId, out var a))
			return;
		
		a = execution.Authors.Affect(AuthorId);

		a.ModerationReward = Amount;
	}
}

public class AuthorOwnerAddition : FairOperation
{
	public AutoId				AuthorId { get; set; }
	public AutoId				Owner { get; set; }

	public override string		Explanation => $"{AuthorId}, {Owner}";
	
	public AuthorOwnerAddition()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		AuthorId	= reader.Read<AutoId>();
		Owner		= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.Write(Owner);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireAuthorAccess(execution, AuthorId, out var a))
			return;
		
		a = execution.Authors.Affect(AuthorId);

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
	public AutoId				AuthorId { get; set; }
	public AutoId				Owner { get; set; }

	public override string		Explanation => $"{AuthorId}, {Owner}";
	
	public AuthorOwnerRemoval()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		AuthorId	= reader.Read<AutoId>();
		Owner		= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(AuthorId);
		writer.Write(Owner);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireAuthorAccess(execution, AuthorId, out var a))
			return;
		
		a = execution.Authors.Affect(AuthorId);

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
