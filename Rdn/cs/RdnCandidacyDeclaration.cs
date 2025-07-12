using System.Net;

namespace Uccs.Rdn;

public class RdnCandidacyDeclaration : CandidacyDeclaration
{
	public IPAddress[]		SeedHubRdcIPs;
	
	public RdnCandidacyDeclaration()
	{
	}

	public RdnCandidacyDeclaration(IPAddress[] baseRdcIPs, IPAddress[] seedHubRdcIPs)
	{
		GraphIPs = baseRdcIPs;
		SeedHubRdcIPs = seedHubRdcIPs;
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		SeedHubRdcIPs = reader.ReadArray(reader.ReadIPAddress);
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(SeedHubRdcIPs, writer.Write);
	}

	public override void Execute(Execution execution)
	{
		base.Execute(execution);

		if(Affected != null)
		{
			(Affected as RdnGenerator).SeedHubRdcIPs = SeedHubRdcIPs;
		}

		execution.PayCycleEnergy(Signer);
	}
}
