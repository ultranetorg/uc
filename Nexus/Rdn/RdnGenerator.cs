using System.Net;

namespace Uccs.Rdn;

public class RdnGenerator : Generator
{
	public Endpoint[] SeedHubPpcIPs { get; set; } = [];

	public override string ToString()
	{
		return $"{base.ToString()}, SeedHubRdcIPs={{{SeedHubPpcIPs.Length}}}";
	}

	public override void WriteMember(Writer writer)
	{
		base.WriteMember(writer);
		writer.Write(SeedHubPpcIPs);
	}

	public override void ReadMember(Reader reader)
	{
		base.ReadMember(reader);
		SeedHubPpcIPs = reader.ReadArray<Endpoint>();
	}

	public override void WriteCandidate(Writer writer)
	{
		base.WriteCandidate(writer);
		writer.Write(SeedHubPpcIPs);
	}

	public override void ReadCandidate(Reader reader)
	{
		base.ReadCandidate(reader);
		SeedHubPpcIPs = reader.ReadArray<Endpoint>();
	}

	public override Generator Clone()
	{
		var g = new RdnGenerator();

		g.SeedHubPpcIPs = SeedHubPpcIPs;

		Clone(g);

		return g;
	}
}
