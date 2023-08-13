using System.IO;

namespace Uccs.Net
{
	public class CandidacyDeclaration : Operation
	{
		public Coin				Bail;
		public override string	Description => $"{Bail} UNT";
		public override bool	Valid => DevSettings.DisableBailMin ? true : Bail >= Mcv.BailMin;
		
		public CandidacyDeclaration()
		{
		}

		public CandidacyDeclaration(AccountKey signer, Coin bail)
		{
			//if(!Settings.Dev.DisableBailMin && bail < Roundchain.BailMin)	throw new RequirementException("The bail must be greater than or equal to BailMin");

			Signer = signer;
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

			if(e.BailStatus == BailStatus.Active) /// first, add existing if not previously Siezed
				e.Balance += e.Bail;

			e.Balance -= Bail; /// then, subtract a new bail
			e.Bail += Bail;

			if(e.BailStatus == BailStatus.Siezed) /// if was siezed than reset to OK status
				e.BailStatus = BailStatus.Active;

			e.CandidacyDeclarationRid = round.Id;
		}
	}
}
