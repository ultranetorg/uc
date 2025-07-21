namespace Uccs.Net;

public class AccountCreation : Operation
{
	public AccountAddress		Owner { get; set; }

	public override string		Explanation => $"{Owner}";
	
	public AccountCreation ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Owner = reader.Read<AccountAddress>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Owner);
	}

	public override void Execute(Execution execution)
	{
		if(execution.FindAccount(Owner) != null)
		{
			Error = AlreadyExists;
			return;
		}

		var a = execution.CreateAccount(Owner);

		if(execution.Round.Id > 0)
		{
			Signer.Spacetime -= execution.Round.AccountAllocationFee(a);

			execution.SpacetimeSpenders.Add(a);
		}
	}
}
