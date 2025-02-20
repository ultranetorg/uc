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

	public override void Execute(Mcv mcv, Round round)
	{
		if(mcv.Accounts.Find(Owner, round.Id) != null)
		{
			Error = AlreadyExists;
			return;
		}

		var a = round.CreateAccount(Owner);

		if(Signer.Address != mcv.Net.God)
		{
			Signer.Spacetime -= round.AccountAllocationFee(a);

			SpacetimeSpenders.Add(a);
		}
	}
}
