using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class CandidacyDeclaration : Operation
	{
		public Money			Bail;
		public override string	Description => $"{Bail} UNT";
		public override bool	Valid => Bail >= Transaction.Zone.BailMin;
		
		public CandidacyDeclaration()
		{
		}

		public CandidacyDeclaration(Money bail)
		{
			Bail = bail;
		}

		public override void ReadConfirmed(BinaryReader r)
		{
			Bail = r.ReadCoin();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(Bail);
		}

		public override void Execute(Mcv chain, Round round)
		{
			if(round.Members.Any(i => i.Account == Signer))
			{
				Error = "Changing candidacy declaration is not allowed while being a member";
				return;
			}

			var e = Affect(round, Signer);

			//var prev = e.ExeFindOperation<CandidacyDeclaration>(round);

			//if(e.BailStatus != BailStatus.Siezed) /// first, return existing if not previously Siezed
			//	e.Balance += e.Bail;

			e.Balance += e.Bail;
			e.Balance -= Bail; /// then, subtract a new bail
			e.Bail += Bail;
			e.CandidacyDeclarationRid = round.Id;
			//e.BailStatus = BailStatus.Active;
		}
	}
}
