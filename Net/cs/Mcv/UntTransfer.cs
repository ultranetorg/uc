using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class UntTransfer : Operation
	{
		public AccountAddress	To;
		public Money			STAmount;
		public Money			EUAmount;
		public Money			MRAmount;
		public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(STAmount > 0 ? STAmount + " ST" : null),
																							  (EUAmount > 0 ? EUAmount + " EU" : null), 
																							  (MRAmount > 0 ? MRAmount + " MR" : null)}.Where(i => i != null))} -> {To}";
		public override bool	IsValid(Mcv mcv) => STAmount > 0 || EUAmount > 0 || MRAmount > 0 ;

		public UntTransfer()
		{
		}

		public UntTransfer(AccountAddress to, Money st, Money eu, Money mr)
		{
			if(to == null)
				throw new RequirementException("Destination account is null or invalid");

			To			= to;
			STAmount	= st;
			EUAmount	= eu;
			MRAmount	= mr;
		}

		public override void ReadConfirmed(BinaryReader r)
		{
			To			= r.ReadAccount();
			STAmount	= r.Read<Money>();
			EUAmount	= r.Read<Money>();
			MRAmount	= r.Read<Money>();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(To);
			w.Write(STAmount);
			w.Write(EUAmount);
			w.Write(MRAmount);
		}

		public override void Execute(Mcv chain, Round round)
		{
			if(Signer != chain.Zone.God || round.Id > Mcv.LastGenesisRound)
			{
				var from = Affect(round, Signer);
				from.STBalance -= STAmount;
				from.EUBalance -= EUAmount;
				from.MRBalance -= MRAmount;
			}
		
			var to = Affect(round, To);
			to.STBalance += STAmount;
			to.EUBalance += EUAmount;
			to.MRBalance += MRAmount;
		}
	}
}
