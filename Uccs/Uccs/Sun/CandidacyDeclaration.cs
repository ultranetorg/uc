using System.IO;

namespace Uccs.Net
{
	public class CandidacyDeclaration : Operation
	{
		public Coin				Bail;
		public override string	Description => $"{Bail} UNT";
		public override bool	Valid => Bail >= Transaction.Zone.BailMin;
		
		public CandidacyDeclaration()
		{
		}

		public CandidacyDeclaration(Coin bail)
		{
			Bail = bail;
		}

		protected override void ReadConfirmed(BinaryReader r)
		{
			Bail = r.ReadCoin();
		}

		protected override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(Bail);
		}

		public override void Execute(Mcv chain, Round round)
		{
			var e = round.AffectAccount(Signer);

			//var prev = e.ExeFindOperation<CandidacyDeclaration>(round);

			if(e.BailStatus != BailStatus.Siezed) /// first, return existing if not previously Siezed
				e.Balance += e.Bail;

			e.Balance -= Bail; /// then, subtract a new bail
			e.Bail += Bail;

			//if(e.BailStatus == BailStatus.Siezed) /// if was siezed than reset to OK status

			e.CandidacyDeclarationRid = round.Id;
			
			e.BailStatus = BailStatus.Active;
		}
	}
}
