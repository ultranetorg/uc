using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public class HubVoxRequest : RdcRequest, IEquatable<HubVoxRequest>
	{
		public int						RoundId { get; set; }
		public IEnumerable<IPAddress>	IPs { get; set; }
		public byte[]					Signature { get; set; }

		public AccountAddress			Account;
		public override bool			WaitResponse => false;

		public HubVoxRequest()
		{
		}

		public override string ToString()
		{
			return base.ToString() + ", Account=" + Account + ", IP=" + string.Join(',', IPs as IEnumerable<IPAddress>);
		}

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Roles.HasFlag(Role.Base))	throw new RdcNodeException(RdcNodeError.NotBase);

				if(core.Synchronization == Synchronization.Null || core.Synchronization == Synchronization.Downloading || core.Synchronization == Synchronization.Synchronizing)
				{
 					var min = core.SyncCache.Any() ? core.SyncCache.Max(i => i.Key) - Database.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
 
					if(RoundId < min || (core.SyncCache.ContainsKey(RoundId) && core.SyncCache[RoundId].HubVoxes.Contains(this)))
						return null;

					Core.SyncRound l;
						
					if(!core.SyncCache.TryGetValue(RoundId, out l))
					{
						l = core.SyncCache[RoundId] = new();
					}

					l.HubVoxes.Add(this);
					
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
					if(RoundId <= core.Database.LastConfirmedRound.Id || core.Database.LastConfirmedRound.Id + Database.Pitch * 2 < RoundId)
						return null;

					var r = core.Database.GetRound(RoundId);
				
					if(!r.HubVoxes.Contains(this))
					{
						r.HubVoxes.Add(this);
				
						var h = core.Database.LastConfirmedRound.Hubs.Find(i => i.Account == Account);
	
						if(h != null)
						{
							h.IPs = IPs.ToArray();
							h.Proxy = Peer;
						}

						foreach(var i in core.Bases.Where(i => i != Peer))
						{
							i.Send(new HubVoxRequest {RoundId = RoundId, IPs = IPs, Signature = Signature});
						}
					}
					//else if(ep.Peers != null && !ep.Peers.Contains(Peer))
					//	ep.BroadcastConfirmed = true;
				}
			}

			return null; 
		}
		
		public byte[] Hashify(Zone zone)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt(RoundId);
			w.Write(IPs, i => w.Write(i));

			return zone.Cryptography.Hash(s.ToArray());
		}
		
		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(RoundId);
			writer.Write(IPs, i => writer.Write(i));
			writer.Write(Signature);
		}
		
		public void Read(BinaryReader reader, Zone zone)
		{
			RoundId		= reader.Read7BitEncodedInt();
			IPs			= reader.ReadArray(() => reader.ReadIPAddress());
			Signature	= reader.ReadSignature();
		
			Account = zone.Cryptography.AccountFrom(Signature, Hashify(zone));
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as HubVoxRequest);
		}

		public bool Equals(HubVoxRequest other)
		{
			return other is not null && Signature.SequenceEqual(other.Signature);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Signature);
		}
	}
}
