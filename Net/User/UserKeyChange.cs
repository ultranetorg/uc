namespace Uccs.Net;

public class UserKeyChange : Operation
{
	public PublicKey			Key { get; set; }

	public override string		Explanation => $"{Key}";
	
	public UserKeyChange()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(Reader reader)
	{
		Key = reader.Read<PublicKey>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Key);
	}

	public override void Execute(Execution execution)
	{
		User.Key = Key;
	}
}
