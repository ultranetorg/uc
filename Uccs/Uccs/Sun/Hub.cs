using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class Hub
	{
		public AccountAddress			Account { get; set; }
		public IEnumerable<IPAddress>	IPs { get; set; } = new IPAddress[0];
		public byte						Bucket { get; set; }
		public int						JoinedAt;
		public Peer         			Proxy;
	
  		public void WriteConfirmed(BinaryWriter w)
 		{
 			w.Write(Account);
 		}
 
 		public void ReadConfirmed(BinaryReader r)
 		{
			Account	= r.ReadAccount();
		}
	
  		public void WriteForBase(BinaryWriter w)
 		{
 			w.Write(Account);
 			w.Write(Bucket);
			w.Write7BitEncodedInt(JoinedAt);
 		}
 
 		public void ReadForBase(BinaryReader r)
 		{
			Account		= r.ReadAccount();
			Bucket		= r.ReadByte();
 			JoinedAt	= r.Read7BitEncodedInt();
		}

		public override string ToString()
		{
			return $"Account={Account}, Bucket={Bucket}, IPs={{{IPs.Count()}}}, JoinedAt={JoinedAt}";
		}
	}
}
