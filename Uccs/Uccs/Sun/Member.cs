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
		public AccountAddress		Generator { get; set; }
		public IPAddress[]			IPs { get; set; } = new IPAddress[0];
		//public ChainTime			OnlineSince { get; set; }
		public int					ActivatedAt;
		public List<IPAddress>		Proxies = new ();
	
  		public void WriteConfirmed(BinaryWriter w)
 		{
 			w.Write(Generator);
			//w.Write7BitEncodedInt(ActivatedAt);
 			//w.Write(IPs, i => w.Write(i));
 		}
 
 		public void ReadConfirmed(BinaryReader r)
 		{
			Generator	= r.ReadAccount();
 			//ActivatedAt	= r.Read7BitEncodedInt();
 			//IPs		= r.ReadArray(() => r.ReadIPAddress());
		}
	
  		public void WriteForBase(BinaryWriter w)
 		{
 			w.Write(Generator);
			w.Write7BitEncodedInt(ActivatedAt);
 			//w.Write(IPs, i => w.Write(i));
 		}
 
 		public void ReadForBase(BinaryReader r)
 		{
			Generator	= r.ReadAccount();
 			ActivatedAt	= r.Read7BitEncodedInt();
 			//IPs		= r.ReadArray(() => r.ReadIPAddress());
		}
	
  		public void WriteForSharing(BinaryWriter w)
 		{
 			w.Write(Generator);
 			w.Write(IPs, i => w.Write(i));
 		}
 
 		public void ReadForSharing(BinaryReader r)
 		{
			Generator	= r.ReadAccount();
			IPs			= r.ReadArray(() => r.ReadIPAddress());
		}

		public override string ToString()
		{
			return $"Generator={Generator}, ActivatedAt={ActivatedAt}}}";
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

		public override void Send(RdcRequest rq)
		{
			Rdi.Send(rq);
		}
	}
}
