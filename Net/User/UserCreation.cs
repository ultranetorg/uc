using System.Numerics;

namespace Uccs.Net;

public class UserCreation : Operation
{
	public PublicKey			Owner { get; set; }
	
	public override string		Explanation => $"{nameof(Owner)}={Owner}";
	
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
		if(User.Key != null)
		{
			Error = AlreadyExists;
			return;
		}

		User.Key = Owner;
	}
}
