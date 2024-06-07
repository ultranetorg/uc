using System.IO;

namespace Uccs.Net
{
	public class UntTransfer : Operation
	{
		public AccountAddress	To;
		public Money			Amount;
		public override string	Description => $"{Signer} -> {Amount} UNT -> {To}";
		public override bool	IsValid(Mcv mcv) => 0 <= Amount;

		public UntTransfer()
		{
		}

		public UntTransfer(AccountAddress to, Money amount)
		{
			if(to == null)		throw new RequirementException("Destination account is null or invalid");

			To		= to;
			Amount	= amount;
		}

		public override void ReadConfirmed(BinaryReader r)
		{
			To		= r.ReadAccount();
			Amount	= r.Read<Money>();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(To);
			w.Write(Amount);
		}

		public override void Execute(Mcv chain, Round round)
		{
			var from = Affect(round, Signer);
			var to = Affect(round, To);

			from.Balance -= Amount;
			to.Balance += Amount;
		}
	}
}
