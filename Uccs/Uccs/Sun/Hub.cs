using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Uccs.Net
{
	public class Hub
	{
		public AccountAddress			Account { get; set; }
		public IEnumerable<IPAddress>	IPs { get; set; } = new IPAddress[0];
		public byte						Cluster { get; set; }
		public byte						ClusterMusk { get; set; }
		public int						JoinedAt;
	
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
 			w.Write(Cluster);
 			w.Write(ClusterMusk);
			w.Write7BitEncodedInt(JoinedAt);
 		}
 
 		public void ReadForBase(BinaryReader r)
 		{
			Account		= r.ReadAccount();
			Cluster		= r.ReadByte();
			ClusterMusk	= r.ReadByte();
 			JoinedAt	= r.Read7BitEncodedInt();
		}

		public override string ToString()
		{
			return $"Account={Account}, Cluster={Convert.ToString(Cluster, 2)}, ClusterMusk={Convert.ToString(ClusterMusk, 2)}, IPs={{{IPs.Count()}}}, JoinedAt={JoinedAt}";
		}
	}
}
