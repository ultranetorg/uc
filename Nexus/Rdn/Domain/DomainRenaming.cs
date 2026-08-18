namespace Uccs.Rdn;

public class DomainRenaming :RdnOperation
{
	public AutoId				Domain { get; set; }
	public string				NewName { get; set; }

	public override string		Explanation => $"{nameof(Domain)}={Domain}, {nameof(NewName)}={NewName}";
	
	public override bool		IsValid(McvNet net) => Uccs.Net.User.IsNameValid(NewName) && DomainName.IsRoot(NewName);

	public DomainRenaming()
	{
	}

	public override void Read(Reader reader)
	{
		Domain = reader.Read<AutoId>();
		NewName = reader.ReadASCII();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Domain);
		writer.WriteASCII(NewName);
	}

	public override void Execute(RdnExecution execution)
	{
		if(!RequireDomainAccess(execution, Domain, out var d))
			return;

		if(!RequireDomainNameAccess(execution, NewName, out var a))
			return;

		d = execution.Domains.Affect(d.Id);
		a = execution.DomainNames.Affect(a.Id);

		d.Name = NewName;
		a.Domain = d.Id;
	
		execution.PayOperationEnergy(User);
	}
}
