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

	public override void Read(Reader reader)
	{
		AuthorId	= reader.Read<AutoId>();
		Years		= reader.ReadByte();
	}

	public override void Write(Writer writer)
	{
		writer.Write(AuthorId);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, AuthorId, out var a, out Error))
			return;
		
		a = execution.Authors.Affect(AuthorId);

		if(!(a as IExpirable).CanRenew(execution.Time, Time.FromYears(Years)))
		{
			Error = NotAvailable;
			return;
		}

		execution.Prolong(a, a, Time.FromYears(Years));
		execution.PayOperationEnergy(a);
	}
}

public class AuthorModerationReward : FairOperation
{
	public AutoId				AuthorId { get; set; }
	public long					Energy { get; set; }

	public override string		Explanation => $"{AuthorId}, {Energy}";
	
	public AuthorModerationReward()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(Reader reader)
	{
		AuthorId	= reader.Read<AutoId>();
		Energy		= reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write(AuthorId);
		writer.Write7BitEncodedInt64(Energy);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, AuthorId, out var a, out Error))
			return;
		
		a = execution.Authors.Affect(AuthorId);

		a.ModerationReward = Energy;

		execution.PayOperationEnergy(a);
	}
}

public class PublisherLimitsUpdation : FairOperation
{
	public AutoId				Author { get; set; }
	public AutoId				Store { get; set; }
	public long					EnergyLimit { get; set; }
	public long					SpacetimeLimit { get; set; }

	public override string		Explanation => $"{nameof(Author)}={Author}, {nameof(Store)}={Store}, {nameof(EnergyLimit)}={EnergyLimit}, {nameof(SpacetimeLimit)}={SpacetimeLimit}";
	
	public PublisherLimitsUpdation()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(Reader reader)
	{
		Author			= reader.Read<AutoId>();
		Store			= reader.Read<AutoId>();
		EnergyLimit		= reader.Read7BitEncodedInt64();
		SpacetimeLimit	= reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Author);
		writer.Write(Store);
		writer.Write7BitEncodedInt64(EnergyLimit);
		writer.Write7BitEncodedInt64(SpacetimeLimit);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		if(!StoreExists(execution, Store, out var s, out Error))
			return;
		
		s = execution.Stores.Affect(s.Id);

		var i = Array.FindIndex(s.Publishers, i => i.Author == a.Id);

		if(i == -1)
		{
			Error = NotFound;
			return;
		}

		s.Publishers = [..s.Publishers];
		var b = s.Publishers[i] = s.Publishers[i].Clone(); 

		b.EnergyLimit = EnergyLimit;
		b.SpacetimeLimit = SpacetimeLimit;

		a = execution.Authors.Affect(a.Id);
		execution.PayOperationEnergy(a);
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

	public override void Read(Reader reader)
	{
		AuthorId	= reader.Read<AutoId>();
		Owner		= reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(AuthorId);
		writer.Write(Owner);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, AuthorId, out var a, out Error))
			return;
		
		a = execution.Authors.Affect(AuthorId);

		if(!UserExists(execution, Owner, out var x, out Error))
			return;

		a.Owners = [..a.Owners, x.Id];

		execution.PayOperationEnergy(a);
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

	public override void Read(Reader reader)
	{
		AuthorId	= reader.Read<AutoId>();
		Owner		= reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(AuthorId);
		writer.Write(Owner);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, AuthorId, out var a, out Error))
			return;
		
		a = execution.Authors.Affect(AuthorId);

		if(a.Owners.Length == 1)
		{
			Error = AtLeastOneOwnerRequired;
			return;
		}

		if(!UserExists(execution, Owner, out var x, out Error))
			return;

		a.Owners = a.Owners.Remove(x.Id);

		execution.PayOperationEnergy(a);
	}
}
