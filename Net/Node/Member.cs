using System.IO;
using System.Net;

namespace Uccs.Net
{
	public class Member
	{
		public AccountAddress	Account { get; set; }
		public Money			Bail { get; set; }
		public IPAddress[]		BaseRdcIPs { get; set; } = new IPAddress[0];
		public IPAddress[]		SeedHubRdcIPs { get; set; } = new IPAddress[0];
		public int				CastingSince { get; set; }
		public Peer         	Proxy;

		public override string ToString()
		{
			return $"Account={Account}, JoinedAt={CastingSince}, BaseRdcIPs={{{BaseRdcIPs.Length}}}, SeedHubRdcIPs={{{SeedHubRdcIPs.Length}}}";
		}
	
  		public void WriteBaseState(BinaryWriter writer)
 		{
 			writer.Write(Account);
			writer.Write(Bail);
			writer.Write(BaseRdcIPs, i => writer.Write(i));
			writer.Write(SeedHubRdcIPs, i => writer.Write(i));
			writer.Write7BitEncodedInt(CastingSince);
 		}
 
 		public void ReadBaseState(BinaryReader reader)
 		{
			Account			= reader.ReadAccount();
			Bail			= reader.Read<Money>();
			BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
			SeedHubRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
 			CastingSince	= reader.Read7BitEncodedInt();
		}
	}
}
