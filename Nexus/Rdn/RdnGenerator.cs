using System.Net;

namespace Uccs.Rdn;

public class RdnGenerator : Generator
{
	public Endpoint[] SeedhubPpiEndpoints { get; set; } = [];

	public override string ToString()
	{
		return $"{base.ToString()}, SeedHubRdcIPs={{{SeedhubPpiEndpoints.Length}}}";
	}

	public override void WriteMember(Writer writer)
	{
		base.WriteMember(writer);
		writer.Write(SeedhubPpiEndpoints);
	}

	public override void ReadMember(Reader reader)
	{
		base.ReadMember(reader);
		SeedhubPpiEndpoints = reader.ReadArray<Endpoint>();
	}

	public override void WriteCandidate(Writer writer)
	{
		base.WriteCandidate(writer);
		writer.Write(SeedhubPpiEndpoints);
	}

	public override void ReadCandidate(Reader reader)
	{
		base.ReadCandidate(reader);
		SeedhubPpiEndpoints = reader.ReadArray<Endpoint>();
	}

	public override Generator Clone()
	{
		var g = new RdnGenerator();

		g.SeedhubPpiEndpoints = SeedhubPpiEndpoints;

		Clone(g);

		return g;
	}
}
