using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class UnitTransfer : Operation
	{
		public AccountAddress	To;
		public Unit				STAmount;
		public Unit				EUAmount;
		public Unit				MRAmount;
		public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(STAmount > 0 ? STAmount + " ST" : null),
																							  (EUAmount > 0 ? EUAmount + " EU" : null), 
																							  (MRAmount > 0 ? MRAmount + " MR" : null)}.Where(i => i != null))} -> {To}";
		public override bool	IsValid(Mcv mcv) => STAmount > 0 || EUAmount > 0 || MRAmount > 0 ;

		public UnitTransfer()
		{
		}

		public UnitTransfer(AccountAddress to, Unit st, Unit eu, Unit mr)
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
				Signer.BYBalance -= STAmount;
				Signer.ECBalance -= EUAmount;
				Signer.MRBalance -= MRAmount;
			}
		
			var to = Affect(round, To);

			to.BYBalance += STAmount;
			to.ECBalance += EUAmount;
			to.MRBalance += MRAmount;
		}
	}
}
