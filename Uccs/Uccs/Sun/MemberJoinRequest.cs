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

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				if(Generator == null)
				{
					Generator = sun.Zone.Cryptography.AccountFrom(Signature, Hashify(sun.Zone));
				}

				if(sun.Synchronization == Synchronization.None || sun.Synchronization == Synchronization.Downloading || sun.Synchronization == Synchronization.Synchronizing)
				{
	 				var min = sun.SyncCache.Any() ? sun.SyncCache.Max(i => i.Key) - Mcv.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
	 
					if(RoundId < min || (sun.SyncCache.ContainsKey(RoundId) && sun.SyncCache[RoundId].Joins.Any(i => i.Generator == Generator)))
					{
						return null;
					}
	
					Sun.SyncRound l;
							
					if(!sun.SyncCache.TryGetValue(RoundId, out l))
					{
						l = sun.SyncCache[RoundId] = new();
					}
	
					l.Joins.Add(this);
						
					foreach(var i in sun.SyncCache.Keys)
					{
						if(i < min)
						{
							sun.SyncCache.Remove(i);
						}
					}					
				}
				else if(sun.Synchronization == Synchronization.Synchronized)
				{
					var d = sun.Mcv;
	
					for(int i = RoundId; i > RoundId - Mcv.Pitch * 2; i--) /// not more than 1 request per [2 x Pitch] rounds
						if(d.FindRound(i) is Round r && r.JoinRequests.Any(j => j.Generator == Generator))
							return null;

					var a = sun.Mcv.Accounts.Find(Generator, sun.Mcv.LastConfirmedRound.Id);
					
					if(a == null || a.Bail < sun.Zone.BailMin /*|| a.BailStatus != BailStatus.Active*/)
						return null;
					
					sun.Mcv.GetRound(RoundId).JoinRequests.Add(this);
					sun.Mcv.JoinAdded?.Invoke(this);
				}
	
				foreach(var i in sun.Connections.Where(i => i != Peer))
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
