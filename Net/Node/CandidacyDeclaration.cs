using System.IO;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public class CandidacyDeclaration : Operation
	{
		public Money			Bail;
		public IPAddress[]		BaseRdcIPs;
		public IPAddress[]		SeedHubRdcIPs;
		public override string	Description => $"{Bail} UNT";
		public override bool	IsValid(Mcv mcv) => Bail >= Transaction.Zone.BailMin;
		
		public CandidacyDeclaration()
		{
		}

		public CandidacyDeclaration(Money bail, IPAddress[] baseRdcIPs, IPAddress[] seedHubRdcIPs)
		{
			Bail = bail;
			BaseRdcIPs = baseRdcIPs;
			SeedHubRdcIPs = seedHubRdcIPs;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Bail			= reader.Read<Money>();
			BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
			SeedHubRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Bail);
			writer.Write(BaseRdcIPs, i => writer.Write(i));
			writer.Write(SeedHubRdcIPs, i => writer.Write(i));
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(round.Members.Any(i => i.Account == Signer))
			{
				Error = "Changing candidacy declaration is not allowed while being a member";
				return;
			}

			
			Affect(round, Signer).Balance -= Bail;

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
