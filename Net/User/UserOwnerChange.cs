namespace Uccs.Net;

public class UserOwnerChange : Operation
{
	public PublicKey		Owner { get; set; }

	public override string		Explanation => $"{Owner}";
	
	public UserOwnerChange()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(Reader reader)
	{
		Owner = reader.Read<PublicKey>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Owner);
	}

	public override void Execute(Execution execution)
	{
		User.Owner = Owner;
	}
}
