using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Reflection.Emit;

namespace Uccs.Net
{
	public class GeneratorOnlineBroadcastRequest : RdcRequest
	{
		public ChainTime				Time { get; set; }
		public IEnumerable<IPAddress>	IPs { get; set; } 
		public byte[]					Signature { get; set; }

		public AccountAddress			Account;
		public override bool			WaitResponse => false;

		public GeneratorOnlineBroadcastRequest()
		{
		}

		public override string ToString()
		{
			return base.ToString()  + ", Account=" + Account  /* + ", IP=" + string.Join(',', IPs as IEnumerable<IPAddress>)*/;
		}

		public override RdcResponse Execute(Core core)
		{
			if(Account == null)
			{
				Account = core.Zone.Cryptography.AccountFrom(Signature, Hashify(core.Zone));
			}

			lock(core.Lock)
			{
				var m = core.Members.Find(i => i.Generator == Account);

				if(m == null)
				{
					if(core.Settings.Roles.HasFlag(Role.Base))
					{
						if(core.Synchronization == Synchronization.Synchronized && !core.Database.LastConfirmedRound.Members.Any(i => i.Generator == Account))
							return null;
					}

					m = new Member{Generator = Account};
					core.Members.Add(m)	;
				}

				if(IPs.Any())
				{
					if(!m.IPs.SequenceEqual(IPs))
					{
						m.IPs = IPs.ToArray();
		
						foreach(var i in core.Connections.Where(i => i != Peer))
							i.Send(new GeneratorOnlineBroadcastRequest {Account = Account, Time = Time, IPs = IPs, Signature = Signature});
					}
				}
				else if(m.Proxy == null || m.OnlineSince < Time)
				{
					foreach(var i in core.Connections.Where(i => i != Peer))
						i.Send(new GeneratorOnlineBroadcastRequest {Account = Account, Time = Time, IPs = IPs, Signature = Signature});
				
					m.Proxy = Peer;
					m.OnlineSince = Time;
				}

				return null;
			}
		}
		
		public byte[] Hashify(Zone zone)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			//writer.Write(Generator);

			//w.Write(Time);
			w.Write(IPs, i => w.Write(i));

			return zone.Cryptography.Hash(s.ToArray());
		}
		
		public void Write(BinaryWriter writer)
		{
			//writer.Write(Time);
			writer.Write(IPs, i => writer.Write(i));
			writer.Write(Signature);
		}
		
		public void Read(BinaryReader reader, Zone zone)
		{
			//Time		= reader.ReadTime();
			IPs			= reader.ReadArray(() => reader.ReadIPAddress());
			Signature	= reader.ReadSignature();
		
			Account = zone.Cryptography.AccountFrom(Signature, Hashify(zone));
		}
		
		public void Sign(Zone zone, AccountKey generator)
		{
			Account = generator;
			Signature = zone.Cryptography.Sign(generator, Hashify(zone));
		}
	}
}
