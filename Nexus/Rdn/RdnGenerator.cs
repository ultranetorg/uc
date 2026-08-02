using System.Net;

namespace Uccs.Rdn;

public class RdnGenerator : Generator
{
	public Endpoint[] SeedhubPpiEndpoints { get; set; } = [];

	public override string ToString()
	{
		return $"{base.ToString()}, SeedHubRdcIPs={{{SeedhubPpiEndpoints.Length}}}";
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.Write(SeedhubPpiEndpoints);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
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
