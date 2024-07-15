using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class UntTransfer : Operation
	{
		public AccountAddress	To;
		public Unit			STAmount;
		public Unit			EUAmount;
		public Unit			MRAmount;
		public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(STAmount > 0 ? STAmount + " ST" : null),
																							  (EUAmount > 0 ? EUAmount + " EU" : null), 
																							  (MRAmount > 0 ? MRAmount + " MR" : null)}.Where(i => i != null))} -> {To}";
		public override bool	IsValid(Mcv mcv) => STAmount > 0 || EUAmount > 0 || MRAmount > 0 ;

		public UntTransfer()
		{
		}

		public UntTransfer(AccountAddress to, Unit st, Unit eu, Unit mr)
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
			STAmount	= r.Read<Unit>();
			EUAmount	= r.Read<Unit>();
			MRAmount	= r.Read<Unit>();
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
			if(Signer.Address != chain.Zone.God || round.Id > Mcv.LastGenesisRound)
			{
				Signer.STBalance -= STAmount;
				Signer.EUBalance -= EUAmount;
				Signer.MRBalance -= MRAmount;
			}
		
			var to = Affect(round, To);

			to.STBalance += STAmount;
			to.EUBalance += EUAmount;
			to.MRBalance += MRAmount;
		}
	}
}
