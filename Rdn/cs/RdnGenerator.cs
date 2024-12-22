using System.Net;

namespace Uccs.Rdn;

public class RdnGenerator : Generator
{
	public IPAddress[]		SeedHubRdcIPs { get; set; } = [];

	public override string ToString()
	{
		return $"{base.ToString()}, SeedHubRdcIPs={{{SeedHubRdcIPs.Length}}}";
	}

  		public override void WriteMember(BinaryWriter writer)
 		{
		base.WriteMember(writer);
		
		writer.Write(SeedHubRdcIPs, i => writer.Write(i));
 		}
 
 		public override void ReadMember(BinaryReader reader)
 		{
		base.ReadMember(reader);

		SeedHubRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
	}

  		public override void WriteCandidate(BinaryWriter writer)
 		{
		base.WriteCandidate(writer);
		
		writer.Write(SeedHubRdcIPs, i => writer.Write(i));
 		}
 
 		public override void ReadCandidate(BinaryReader reader)
 		{
		base.ReadCandidate(reader);

		SeedHubRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
	}

	public override Generator Clone()
	{
		var g = new RdnGenerator();

		g.SeedHubRdcIPs = SeedHubRdcIPs;

		Clone(g);

		return g;
	}
}
