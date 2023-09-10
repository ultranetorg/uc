using System.IO;

namespace Uccs.Net
{
	public class UntTransfer : Operation
	{
		public AccountAddress	To;
		public Coin				Amount;
		public override string	Description => $"{Signer} -> {Amount} UNT -> {To}";
		public override bool	Valid => 0 <= Amount;

		public UntTransfer()
		{
		}

		public UntTransfer(AccountAddress to, Coin amount)
		{
			if(to == null)		throw new RequirementException("Destination account is null or invalid");

			To		= to;
			Amount	= amount;
		}

		protected override void ReadConfirmed(BinaryReader r)
		{
			To		= r.ReadAccount();
			Amount	= r.ReadCoin();
		}

		protected override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(To);
			w.Write(Amount);
		}

		public override void Execute(Mcv chain, Round round)
		{
			var from = round.AffectAccount(Signer);

			var newto = chain.Accounts.Find(To, round.Id) == null;
			var to = round.AffectAccount(To);

			from.Balance -= Amount;
			
			if(newto)
			{
				PayForAllocation(0, 10);
			}

			to.Balance += Amount;
		}
	}
}
