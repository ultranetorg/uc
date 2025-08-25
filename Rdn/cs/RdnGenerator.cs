using System.Net;

namespace Uccs.Rdn;

public class RdnGenerator : Generator
{
	public IPAddress[]		SeedHubPpcIPs { get; set; } = [];

	public override string ToString()
	{
		return $"{base.ToString()}, SeedHubRdcIPs={{{SeedHubPpcIPs.Length}}}";
	}

  		public override void WriteMember(BinaryWriter writer)
 		{
		base.WriteMember(writer);
		
		writer.Write(SeedHubPpcIPs, i => writer.Write(i));
 		}
 
 		public override void ReadMember(BinaryReader reader)
 		{
		base.ReadMember(reader);

		SeedHubPpcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
	}

  		public override void WriteCandidate(BinaryWriter writer)
 		{
		base.WriteCandidate(writer);
		
		writer.Write(SeedHubPpcIPs, i => writer.Write(i));
 		}
 
 		public override void ReadCandidate(BinaryReader reader)
 		{
		base.ReadCandidate(reader);

		SeedHubPpcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
	}

	public override Generator Clone()
	{
		var g = new RdnGenerator();

		g.SeedHubPpcIPs = SeedHubPpcIPs;

		Clone(g);

		return g;
	}
}
