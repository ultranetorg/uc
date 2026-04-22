namespace Uccs.Rdn;

public class DomainMigration : OutwardOperation
{ 
	public string			Name  { get; set; }
	public string			Tld  { get; set; }

	public override string	Explanation => $"{Name}.{Tld}";

	public DomainMigration()
	{
	}

	public DomainMigration(string name, string tld)
	{
		Name = name;
		Tld = tld;
	}

	public override bool IsValid(McvNet net)
	{
		if(!Domain.Valid(Name))
			return false;

		if(!Domain.ExclusiveTlds.Contains(Tld))
			return false;

		return true;
	}
	
	public override void Read(BinaryReader reader)
	{
		Name		= reader.ReadUtf8();
		Tld			= reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Name);
		writer.WriteUtf8(Tld);
	}

	//public void WriteBaseState(BinaryWriter writer)
	//{
	//	writer.Write(UserId);
	//	writer.WriteUtf8(Name);
	//	writer.Write(Generator);
	//}
	//
	//public void ReadBaseState(BinaryReader reader)
	//{
	//	UserId		= reader.Read<AutoId>();
	//	Name		= reader.ReadUtf8();
	//	Generator	= reader.Read<AutoId>();
	//}

	public override void Execute(RdnExecution execution)
	{
		var a = execution.Domains.Find(Name);

		if(a?.Owner != null)
		{
			Error = AlreadyTaken;
			return;
		}

		var existing = DomainExecution.Priority.FirstOrDefault(i => i.Value.Contains(Name));

		if(existing.Key == null)
		{
			Error = NotFound;
			return;
		}

		if(existing.Key != Tld)
		{
			Error = OtherTldHasPriority;
			return;
		}

		execution.AffectOutwards();
		execution.Outwards.Add(	new Outward(execution.Net)
								{
									Id			= ++User.LastOutward,
									User		= User.Id, 
									Generator	= Transaction.Member,  
									Operation	= this,
									Expiration	= execution.Time + execution.Net.ForeignVerificationDurationLimit
								 });

	
		execution.PayOperationEnergy(User);
	}

	public override void ConfirmedExecute(RdnExecution execution, Outward task)
	{
		var e = execution as RdnExecution;
		var a = e.Domains.Affect(Name);

		a.Owner = task.User;
	}
}
