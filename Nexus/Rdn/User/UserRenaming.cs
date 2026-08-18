namespace Uccs.Rdn;

public class UserRenaming : RdnOperation
{
	public string				Name { get; set; }

	public override string		Explanation => $"{Name}";
	
	public override bool		IsValid(McvNet net) => Uccs.Net.User.IsNameValid(Name);

	public UserRenaming()
	{
	}

	public override void Read(Reader reader)
	{
		Name = reader.ReadASCII();
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(Name);
	}

	public override void Execute(RdnExecution execution)
	{
		var e = execution.UserNames.Find(Name);

		if(e != null)
		{
			Error = NotAvailable;
			return;
		}

		execution.UserNames.Unregister(User.Name);

		User.Name = Name;
	
		execution.UserNames.Register(User.Name, User.Id);

		execution.PayOperationEnergy(User);
	}
}
