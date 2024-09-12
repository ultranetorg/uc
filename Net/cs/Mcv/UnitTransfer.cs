using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class UnitTransfer : Operation
	{
		public AccountAddress	To;
		public long				BYAmount;
		public long				ECAmount;
		public long				MRAmount;
		public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(BYAmount > 0 ? BYAmount + " BY" : null),
																							  (ECAmount > 0 ? ECAmount + " EC" : null), 
																							  (MRAmount > 0 ? MRAmount + " MR" : null)}.Where(i => i != null))} -> {To}";
		public override bool	IsValid(Mcv mcv) => BYAmount > 0 || ECAmount > 0 || MRAmount > 0 ;

		public UnitTransfer()
		{
		}

		public UnitTransfer(AccountAddress to, long by, long ec, long mr)
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
			BYAmount	= r.Read7BitEncodedInt64();
			ECAmount	= r.Read7BitEncodedInt64();
			MRAmount	= r.Read7BitEncodedInt64();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(To);
			w.Write7BitEncodedInt64(BYAmount);
			w.Write7BitEncodedInt64(ECAmount);
			w.Write7BitEncodedInt64(MRAmount);
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
