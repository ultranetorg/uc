using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class MemberJoinRequest : RdcRequest
	{
		//public IEnumerable<IPAddress>	IPs { get; set; }
		public int						RoundId { get; set; }
		public byte[]					Signature { get; set; }

		public AccountAddress			Generator;
		public override bool			WaitResponse => false;

		public MemberJoinRequest()
		{
		}

		public override string ToString()
		{
			return base.ToString()  + ", Generator=" + Generator  /* + ", IP=" + string.Join(',', IPs as IEnumerable<IPAddress>)*/;
		}
		
		public void Sign(Zone zone, AccountKey generator)
		{
			Generator = generator;
			Signature = zone.Cryptography.Sign(generator, Hashify(zone));
		}

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(Generator == null)
				{
					Generator = core.Zone.Cryptography.AccountFrom(Signature, Hashify(core.Zone));
				}

				if(core.Synchronization == Synchronization.Null || core.Synchronization == Synchronization.Downloading || core.Synchronization == Synchronization.Synchronizing)
				{
	 				var min = core.SyncCache.Any() ? core.SyncCache.Max(i => i.Key) - Database.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
	 
					if(RoundId < min || (core.SyncCache.ContainsKey(RoundId) && core.SyncCache[RoundId].GeneratorJoins.Any(i => i.Generator == Generator)))
					{
						return null;
					}
	
					Core.SyncRound l;
							
					if(!core.SyncCache.TryGetValue(RoundId, out l))
					{
						l = core.SyncCache[RoundId] = new();
					}
	
					l.GeneratorJoins.Add(this);
						
					foreach(var i in core.SyncCache.Keys)
					{
						if(i < min)
						{
							core.SyncCache.Remove(i);
						}
					}					
				}
				else if(core.Synchronization == Synchronization.Synchronized)
				{
					var d = core.Database;
	
					for(int i = RoundId; i > RoundId - Database.Pitch * 2; i--) /// not more than 1 request per [2 x Pitch] rounds
						if(d.FindRound(i) is Round r && r.JoinRequests.Any(j => j.Generator == Generator))
							return null;

					var a = core.Database.Accounts.Find(Generator, core.Database.LastConfirmedRound.Id);
					
					if(a == null || a.Bail == 0 /*|| a.BailStatus != BailStatus.Active*/)
						return null;
									
					core.Database.GetRound(RoundId).JoinRequests.Add(this);
				}
	
				foreach(var i in core.Connections.Where(i => i != Peer))
				{
					i.Send(new MemberJoinRequest {RoundId = RoundId, Signature = Signature});
				}
			}
	
			return null;
		}

		public byte[] Hashify(Zone zone)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			//writer.Write(Generator);

			w.Write7BitEncodedInt(RoundId);
			//w.Write(IPs, i => w.Write(i));

			return zone.Cryptography.Hash(s.ToArray());
		}
		
		public void Write(BinaryWriter writer)
		{
			//writer.Write(IPs, i => writer.Write(i));
			writer.Write(Signature);
		}
		
		public void Read(BinaryReader reader, Zone zone)
		{
			//IPs			= reader.ReadArray(() => reader.ReadIPAddress());
			Signature = reader.ReadSignature();
		
			Generator = zone.Cryptography.AccountFrom(Signature, Hashify(zone));
		}
	}
}
