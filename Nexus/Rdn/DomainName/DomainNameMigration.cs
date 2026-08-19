namespace Uccs.Rdn;

public class DomainNameMigration : RdnOperation, IOutwardOperation
{ 
	public string			Name  { get; set; }
	public string			Tld  { get; set; }

	public override string	Explanation => $"{Name}.{Tld}";

	public DomainNameMigration()
	{
	}

	public DomainNameMigration(string name, string tld)
	{
		Name = name;
		Tld = tld;
	}

	public override bool IsValid(McvNet net)
	{
		if(!(	DomainName.IsAddressValid(Name) &&
				DomainName.IsRoot(Name) && 
				DomainName.PriorityTlds.Contains(Tld)))
			return false;


		var existing = DomainNameTable.Priority.FirstOrDefault(i => i.Value.Contains(Name));

		return existing.Key != null && existing.Key == Tld;
	}
	
	public override void Read(Reader reader)
	{
		Name	= reader.ReadUtf8();
		Tld		= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Name);
		writer.WriteUtf8(Tld);
	}

	public override void Execute(RdnExecution execution)
	{
		if(execution.OutwardTransactions.Count >= McvNet.OutwardsMaximum)
		{
			Error = LimitExceeded;
			return;
		}

		var a = (execution as RdnExecution).DomainNames.Find(Name);

		if(a != null)
		{
			Error = AlreadyTaken;
			return;
		}

		execution.AffectOutwards();
		execution.OutwardTransactions.Add(	new OutwardTransaction
											{
												Id			= ++User.LastOutward,
												User		= User.Id, 
												Operation	= this,
												Expiration	= execution.Time + execution.Net.OutwardVerificationDurationLimit
											 });
	
		execution.PayOperationEnergy(User);
		execution.PayOutwardEnergy(User);
	}

	public void SuccessExecute(Execution execution, OutwardTransaction task)
	{
		var e = execution as RdnExecution;

		if(e.DomainNames.Find(Name) != null) /// Somebody has already migrated this domain
			return;

		var a = e.DomainNames.Create(Name);

		a.Owner = task.User;
	}
}
