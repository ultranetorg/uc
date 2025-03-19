namespace Uccs.Net;

public class AccountCreation : Operation
{
	public AccountAddress		Owner { get; set; }

	public override string		Description => $"{Owner}";
	
	public AccountCreation ()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Owner = reader.Read<AccountAddress>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Owner);
	}

	public override void Execute(Execution execution, Round round)
	{
		if(execution.FindAccount(Owner) != null)
		{
			Error = AlreadyExists;
			return;
		}

		var a = execution.CreateAccount(Owner);

		if(Signer.Address != execution.Net.God)
		{
			Signer.Spacetime -= round.AccountAllocationFee(a);

			SpacetimeSpenders.Add(a);
		}
	}
}
