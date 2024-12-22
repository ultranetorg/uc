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
		BaseRdcIPs = baseRdcIPs;
		SeedHubRdcIPs = seedHubRdcIPs;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		base.ReadConfirmed(reader);

		SeedHubRdcIPs = reader.ReadArray(() => reader.ReadIPAddress());
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		base.WriteConfirmed(writer);

		writer.Write(SeedHubRdcIPs, i => writer.Write(i));
	}

	public override void Execute(Mcv mcv, Round round)
	{
		base.Execute(mcv, round);

		if(Affected != null)
		{
			(Affected as RdnGenerator).SeedHubRdcIPs = SeedHubRdcIPs;
		}
	}
}
