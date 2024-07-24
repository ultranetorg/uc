using System.IO;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public abstract class CandidacyDeclaration : Operation
	{
		public Unit				Pledge;
		public IPAddress[]		BaseRdcIPs;
		public override string	Description => $"{Pledge} UNT";
		public override bool	IsValid(Mcv mcv) => Pledge >= Transaction.Zone.PladgeMin;
		
		public CandidacyDeclaration()
		{
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Pledge			= reader.Read<Unit>();
			BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Pledge);
			writer.Write(BaseRdcIPs, i => writer.Write(i));
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(round.Members.Any(i => i.Account == Signer.Address))
			{
				Error = "Changing candidacy declaration is not allowed while being a member";
				return;
			}
						
			Signer.MRBalance -= Pledge;

			//var e = Affect(round, Signer);

			//var prev = e.ExeFindOperation<CandidacyDeclaration>(round);

			//if(e.BailStatus != BailStatus.Siezed) /// first, return existing if not previously Siezed
			//	e.Balance += e.Bail;

			//e.Balance += e.Bail;
			//e.Balance -= Bail; /// then, subtract a new bail
			//e.Bail = Bail;
			//e.CandidacyDeclarationRid = round.Id;
			//e.BailStatus = BailStatus.Active;
		}
	}
}
