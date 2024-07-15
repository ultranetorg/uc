using System.IO;
using System.Net;

namespace Uccs.Net
{
	public class Member
	{
		public AccountAddress	Account { get; set; }
		public Unit			Bail { get; set; }
		public IPAddress[]		BaseRdcIPs { get; set; } = [];
		public int				CastingSince { get; set; }
		public Peer         	Proxy;

		public override string ToString()
		{
			return $"Account={Account}, JoinedAt={CastingSince}, BaseRdcIPs={{{BaseRdcIPs.Length}}}";
		}
	
  		public virtual void WriteBaseState(BinaryWriter writer)
 		{
 			writer.Write(Account);
			writer.Write(Bail);
			writer.Write(BaseRdcIPs, i => writer.Write(i));
			writer.Write7BitEncodedInt(CastingSince);
 		}
 
 		public virtual void ReadBaseState(BinaryReader reader)
 		{
			Account			= reader.ReadAccount();
			Bail			= reader.Read<Unit>();
			BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
 			CastingSince	= reader.Read7BitEncodedInt();
		}
	}
}
