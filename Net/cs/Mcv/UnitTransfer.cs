using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class UnitTransfer : Operation
	{
		public AccountAddress	To;
		public Unit				BYAmount;
		public Unit				ECAmount;
		public Unit				MRAmount;
		public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(BYAmount > 0 ? BYAmount + " BY" : null),
																							  (ECAmount > 0 ? ECAmount + " EC" : null), 
																							  (MRAmount > 0 ? MRAmount + " MR" : null)}.Where(i => i != null))} -> {To}";
		public override bool	IsValid(Mcv mcv) => BYAmount > 0 || ECAmount > 0 || MRAmount > 0 ;

		public UnitTransfer()
		{
		}

		public UnitTransfer(AccountAddress to, Unit by, Unit ec, Unit mr)
		{
			if(to == null)
				throw new RequirementException("Destination account is null or invalid");

			To			= to;
			BYAmount	= by;
			ECAmount	= ec;
			MRAmount	= mr;
		}

		public override void ReadConfirmed(BinaryReader r)
		{
			To			= r.ReadAccount();
			BYAmount	= r.Read<Unit>();
			ECAmount	= r.Read<Unit>();
			MRAmount	= r.Read<Unit>();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(To);
			w.Write(BYAmount);
			w.Write(ECAmount);
			w.Write(MRAmount);
		}

		public override void Execute(Mcv chain, Round round)
		{
			if(Signer.Address != chain.Zone.God || round.Id > Mcv.LastGenesisRound)
			{
				Signer.BYBalance -= BYAmount;
				Signer.ECBalance -= ECAmount;
				Signer.MRBalance -= MRAmount;
			}
		
			var to = Affect(round, To);

			to.BYBalance += BYAmount;
			to.ECBalance += ECAmount;
			to.MRBalance += MRAmount;
		}
	}
}
