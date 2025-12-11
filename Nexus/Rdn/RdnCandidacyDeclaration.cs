using System.Net;

namespace Uccs.Rdn;

public class RdnCandidacyDeclaration : CandidacyDeclaration
{
	public Endpoint[]		SeedHubRdcIPs;
	
	public RdnCandidacyDeclaration()
	{
	}

	public RdnCandidacyDeclaration(Endpoint[] baseRdcIPs, Endpoint[] seedHubRdcIPs)
	{
		GraphIPs = baseRdcIPs;
		SeedHubRdcIPs = seedHubRdcIPs;
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		SeedHubRdcIPs = reader.ReadArray<Endpoint>();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(SeedHubRdcIPs);
	}

	public override void Execute(Execution execution)
	{
		base.Execute(execution);

		if(Error == null)
		{
			(execution.AffectCandidate(Signer.Id) as RdnGenerator).SeedHubPpcIPs = SeedHubRdcIPs;
		}
	}
}
