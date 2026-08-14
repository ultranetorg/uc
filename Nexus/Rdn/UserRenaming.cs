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
		var e = execution.Names.Find(NameIndex.GetId(Name));

		if(e != null && e.Entities.Any(i => i.Field == EntityTextField.UserName))
		{
			Error = NotAvailable;
			return;
		}

		execution.Names.Unregister(User.Name, EntityTextField.UserName);

		User.Name = Name;
	
		execution.Names.Register(User.Name, EntityTextField.UserName, User.Id);
	}
}
