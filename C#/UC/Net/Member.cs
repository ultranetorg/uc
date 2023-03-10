using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class Member : IBinarySerializable
	{
		public Account				Generator { get; set; }
		public IPAddress[]			IPs { get; set; }
		public int					JoinedAt { get; set; }
	
  		public void Write(BinaryWriter w)
 		{
 			w.Write(Generator);
			w.Write7BitEncodedInt(JoinedAt);
 			w.Write(IPs, i => w.Write(i));
 		}
 
 		public void Read(BinaryReader r)
 		{
			Generator	= r.ReadAccount();
 			JoinedAt	= r.Read7BitEncodedInt();
 			IPs			= r.ReadArray(() => r.ReadIPAddress());
		}

		public override string ToString()
		{
			return $"Generator={Generator}, JoinedAt={JoinedAt}, IPs={{{IPs.Length}}}";
		}
	}

	public class MemberDci : RdcInterface
	{
		public Account			Generator { get; protected set; }
		public RdcInterface		Dci { get; protected set; }

		public MemberDci(Account generator, RdcInterface dci)
		{
			Generator = generator;
			Dci = dci;
		}

		public override Rp Request<Rp>(RdcRequest rq)
		{
			return Dci.Request<Rp>(rq);
		}
	}
}
