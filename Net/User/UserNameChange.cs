namespace Uccs.Net;

public class UserNameChange : Operation
{
	public string		Name { get; set; }

	public override string		Explanation => $"{Name}";
	
	public UserNameChange()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return IsFreeNameValid(Name);
	}

	public override void Read(BinaryReader reader)
	{
		Name = reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteASCII(Name);
	}

	public override void Execute(Execution execution)
	{
		if(execution.FindUser(Name) != null)
		{
			Error = AlreadyExists;
			return;
		}

		User.Name = Name;
	}
}
