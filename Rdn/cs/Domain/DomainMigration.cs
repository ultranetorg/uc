namespace Uccs.Rdn;

public class DomainMigration : RdnOperation
{
	public string			Name  { get; set; }
	public string			Tld  { get; set; }

	public override string	Explanation => $"{Name}.{Tld}";
	public bool				DnsApproved;
	public AutoId			Generator;

	public DomainMigration()
	{
	}

	public DomainMigration(string name, string tld, bool checkrank)
	{
		Name = name;
		Tld = tld;
	}

	public override bool IsValid(McvNet net)
	{
		if(!Domain.Valid(Name))
			return false;

		if(!Domain.IsWeb(Name))
			return false;

		if(Tld.Length > 8)
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

	public void WriteBaseState(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Signer);
		writer.WriteUtf8(Name);
		writer.WriteUtf8(Tld);
		writer.Write(Generator);
	}

	public void ReadBaseState(BinaryReader reader)
	{
		_Id	= reader.Read<OperationId>();

		Transaction = new ();
		
		Transaction.Signer	= reader.ReadAccount();
		Name				= reader.ReadUtf8();
		Tld					= reader.ReadUtf8();
		Generator			= reader.Read<AutoId>();
	}

	public override void Execute(RdnExecution execution)
	{
		var a = execution.Domains.Find(Name);

		if(a?.Owner != null)
		{
			Error = "Already Owned";
			return;
		}

		if(!DomainExecution.Popular.Contains($"{Name}.{Tld}"))
		{
			Error = NotPopularWebDomain;
			return;
		}
	
		execution.PayCycleEnergy(Signer);
	}

	public void ConfirmedExecute(Execution execution)
	{
		var e = execution as RdnExecution;
		var a = e.Domains.Affect(Name);

		a.Owner = Signer.Id;
	}
}
