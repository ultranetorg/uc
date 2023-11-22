using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class Member
	{
		public AccountAddress			Account { get; set; }
		public IPAddress[]				BaseRdcIPs { get; set; } = new IPAddress[0];
		public IPAddress[]				SeedHubRdcIPs { get; set; } = new IPAddress[0];
		public int						CastingSince { get; set; }
		public Peer         			Proxy;

		public override string ToString()
		{
			return $"Account={Account}, JoinedAt={CastingSince}, BaseRdcIPs={{{BaseRdcIPs.Length}}}, SeedHubRdcIPs={{{SeedHubRdcIPs.Length}}}";
		}
	
  		//public void WriteConfirmed(BinaryWriter writer)
 		//{
 		//	writer.Write(Account);
		//	writer.Write(BaseRdcIPs, i => writer.Write(i));
		//	writer.Write(SeedHubRdcIPs, i => writer.Write(i));
		//}
 		//
 		//public void ReadConfirmed(BinaryReader reader)
 		//{
		//	Account			= reader.ReadAccount();
		//	BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
		//	SeedHubRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
		//}
	
  		public void WriteBaseState(BinaryWriter writer)
 		{
 			writer.Write(Account);
			writer.Write(BaseRdcIPs, i => writer.Write(i));
			writer.Write(SeedHubRdcIPs, i => writer.Write(i));
			writer.Write7BitEncodedInt(CastingSince);
 		}
 
 		public void ReadBaseState(BinaryReader reader)
 		{
			Account			= reader.ReadAccount();
			BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
			SeedHubRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
 			CastingSince		= reader.Read7BitEncodedInt();
		}
	
//   		public void WriteForSharing(BinaryWriter w)
//  		{
//  			w.Write(Generator);
//  			w.Write(IPs, i => w.Write(i));
//  		}
//  
//  		public void ReadForSharing(BinaryReader r)
//  		{
// 			Generator	= r.ReadAccount();
// 			IPs			= r.ReadArray(() => r.ReadIPAddress());
// 		}
	}
}
