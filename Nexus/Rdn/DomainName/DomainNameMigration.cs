namespace Uccs.Rdn;

public class DomainNameMigration : RdnOperation, IOutworldOperation
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
		return	DomainName.IsValid(Name) &&
				DomainName.IsRoot(Name) && 
				DomainNameTable.Priority.TryGetValue(Tld, out var n) && n.Contains(Name);
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
		if(execution.OutworldTransactions.Count >= McvNet.OutworldTransactionsMaximum)
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

		execution.AffectOutworlds();
		execution.OutworldTransactions.Add(	new OutworldTransaction
											{
												Id			= ++User.LastOutworld,
												User		= User.Id, 
												Operation	= this,
												Expiration	= execution.Time + execution.Net.OutworldVerificationDurationLimit
											 });
	
		execution.PayOperationEnergy(User);
		execution.PayOutworldEnergy(User);
	}

	public void SuccessExecute(Execution execution, OutworldTransaction task)
	{
		var e = execution as RdnExecution;

		if(e.DomainNames.Find(Name) != null) /// Somebody has already migrated this domain
			return;

		var a = e.DomainNames.Create(Name);

		a.Owner = task.User;
	}
}
