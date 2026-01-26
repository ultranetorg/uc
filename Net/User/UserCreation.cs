using System.Text.RegularExpressions;

namespace Uccs.Net;

public class UserCreation : Operation
{
	public string				Name { get; set; }
	public AccountAddress		Owner { get; set; }

	public override string		Explanation => $"Name={Name} Owner={Owner}";
	
	public UserCreation()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return IsFreeNameValid(Name);
	}

	public override void Read(BinaryReader reader)
	{
		Name = reader.ReadASCII();
		Owner = reader.Read<AccountAddress>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteASCII(Name);
		writer.Write(Owner);
	}

	public override void Execute(Execution execution)
	{
		if(execution.FindUser(Name) != null)
		{
			Error = AlreadyExists;
			return;
		}

		var a = execution.CreateUser(Name, Owner);

		if(execution.Round.Id > 0)
		{
			User.Spacetime -= execution.Round.UserAllocationFee();

			execution.SpacetimeSpenders.Add(User);
		}
	}
}
