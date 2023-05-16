using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Uccs.Net
{
	public class HubOnlineBroadcastRequest : RdcRequest
	{
		public ChainTime				Time { get; set; }
		public IEnumerable<IPAddress>	IPs { get; set; }
		public byte[]					Signature { get; set; }

		public AccountAddress			Account;
		public override bool			WaitResponse => false;

		public HubOnlineBroadcastRequest()
		{
		}

		public override string ToString()
		{
			return base.ToString()  + ", Account=" + Account  /* + ", IP=" + string.Join(',', IPs as IEnumerable<IPAddress>)*/;
		}

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				return null;
			}
		}
		
		public byte[] Hashify(Zone zone)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			//writer.Write(Hub);

			w.Write(Time);
			w.Write(IPs, i => w.Write(i));

			return zone.Cryptography.Hash(s.ToArray());
		}
		
		public void Write(BinaryWriter writer)
		{
			writer.Write(Time);
			writer.Write(IPs, i => writer.Write(i));
			writer.Write(Signature);
		}
		
		public void Read(BinaryReader reader, Zone zone)
		{
			Time		= reader.ReadTime();
			IPs			= reader.ReadArray(() => reader.ReadIPAddress());
			Signature	= reader.ReadSignature();
		
			Account = zone.Cryptography.AccountFrom(Signature, Hashify(zone));
		}
	}
}
