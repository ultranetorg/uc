using System.Net;

namespace Uccs.Net
{
	public abstract class CandidacyDeclaration : Operation
	{
		public IPAddress[]		BaseRdcIPs;

		public override string	Description => $"Id={Signer.Id}, Address={Signer.Address}, BaseRdcIPs={string.Join(',', BaseRdcIPs as object[])}";
				
		protected Generator		Affected;

		public CandidacyDeclaration()
		{
		}

		public override bool IsValid(Mcv mcv) => true;

		public override void ReadConfirmed(BinaryReader reader)
		{
			BaseRdcIPs = reader.ReadArray(() => reader.ReadIPAddress());
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(BaseRdcIPs, i => writer.Write(i));
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(round.Candidates.Count(i => i.Registered == round.ConsensusTime) >= mcv.Zone.CandidatesMaximum)
			{
				Error = "Limit reached";
				return;
			}

			if(round.Members.Any(i => i.Id == Signer.Id))
			{
				Error = "Already member";
				return;
			}

			var c = round.Candidates.Find(i => i.Id == Signer.Id);

			if(c != null && c.Registered == round.ConsensusTime)
			{
				Error = "Already registered";
				return;
			}

			Affected = round.AffectCandidate(Signer.Id);
			
			Affected.Id			= Signer.Id;
			Affected.Address	= Signer.Address;
			Affected.BaseRdcIPs	= BaseRdcIPs;
			Affected.Registered	= round.ConsensusTime;
		}
	}
}
