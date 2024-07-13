using System.IO;
using System.Net;

namespace Uccs.Rdn
{
	public class RdnMember : Member
	{
		public IPAddress[]		SeedHubRdcIPs { get; set; } = [];

		public override string ToString()
		{
			return $"{base.ToString()}, SeedHubRdcIPs={{{SeedHubRdcIPs.Length}}}";
		}
	
  		public override void WriteBaseState(BinaryWriter writer)
 		{
			base.WriteBaseState(writer);
			writer.Write(SeedHubRdcIPs, i => writer.Write(i));
 		}
 
 		public override void ReadBaseState(BinaryReader reader)
 		{
			base.ReadBaseState(reader);
			SeedHubRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
		}
	}
}
