using System.Net;

namespace Uccs.Net
{
	public class Member
	{
		public AccountAddress	Account { get; set; }
		public long				Pledge { get; set; }
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
			writer.Write7BitEncodedInt64(Pledge);
			writer.Write(BaseRdcIPs, i => writer.Write(i));
			writer.Write7BitEncodedInt(CastingSince);
 		}
 
 		public virtual void ReadBaseState(BinaryReader reader)
 		{
			Account			= reader.ReadAccount();
			Pledge			= reader.Read7BitEncodedInt64();
			BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
 			CastingSince	= reader.Read7BitEncodedInt();
		}
	}
}
