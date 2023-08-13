using System.IO;

namespace Uccs.Net
{
	public class UntTransfer : Operation
	{
		public AccountAddress	To;
		public Coin				Amount;
		public override string	Description => $"{Amount} UNT -> {To}";
		public override bool	Valid => 0 <= Amount;

		public UntTransfer()
		{
		}

		public UntTransfer(AccountKey signer, AccountAddress to, Coin amount)
		{
			if(signer == null)	throw new RequirementException("Source account is null or invalid");
			if(to == null)		throw new RequirementException("Destination account is null or invalid");

			Signer = signer;
			To = to;
			Amount = amount;
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

		public override void Execute(Chainbase chain, Round round)
		{
			var s = round.AffectAccount(Signer);
			
			s.Balance -= Amount;

			if(chain.Accounts.Find(To, round.Id) == null)
			{
				var fee = CalculateSpaceFee(round.Factor, CalculateSize(), 10);
				
				s.Balance -= fee;
				round.Fees += fee;
			}

			round.AffectAccount(To).Balance += Amount;
		}
	}
}
