namespace Uccs.Fair;

public class UserRenaming : FairOperation
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

	public override void Execute(FairExecution execution)
	{
		var e = execution.Names.Find(NameIndex.GetId(Name));

		if(e != null)
		{
			Error = NotAvailable;
			return;
		}

		execution.Names.Unregister(User.Name);

		User.Name = Name;
	
		execution.Names.Register(User.Name, EntityTextField.UserName, User.Id);
	}
}
