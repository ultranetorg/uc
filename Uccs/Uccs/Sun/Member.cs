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
		public AccountAddress		Generator { get; set; }
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

	public class MemberRdi : RdcInterface
	{
		public AccountAddress	Generator { get; protected set; }
		public RdcInterface		Rdi { get; protected set; }

		public MemberRdi(AccountAddress generator, RdcInterface rdi)
		{
			Generator = generator;
			Rdi = rdi;
		}

		public override Rp Request<Rp>(RdcRequest rq)
		{
			return Rdi.Request<Rp>(rq);
		}
	}
}
