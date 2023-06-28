using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class Generator
	{
		public AccountAddress			Account;
		public IEnumerable<IPAddress>	IPs  = new IPAddress[0];
		//public ChainTime				OnlineSince = ChainTime.Zero;
		public int						JoinedAt;
		public Peer         			Proxy;
	
  		public void WriteConfirmed(BinaryWriter w)
 		{
 			w.Write(Account);
			//w.Write7BitEncodedInt(ActivatedAt);
 			//w.Write(IPs, i => w.Write(i));
 		}
 
 		public void ReadConfirmed(BinaryReader r)
 		{
			Account	= r.ReadAccount();
 			//ActivatedAt	= r.Read7BitEncodedInt();
 			//IPs		= r.ReadArray(() => r.ReadIPAddress());
		}
	
  		public void WriteForBase(BinaryWriter w)
 		{
 			w.Write(Account);
			w.Write7BitEncodedInt(JoinedAt);
 			//w.Write(IPs, i => w.Write(i));
 		}
 
 		public void ReadForBase(BinaryReader r)
 		{
			Account	= r.ReadAccount();
 			JoinedAt	= r.Read7BitEncodedInt();
 			//IPs		= r.ReadArray(() => r.ReadIPAddress());
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

		public override string ToString()
		{
			return $"Generator={Account}, ActivatedAt={JoinedAt}";
		}
	}
}
