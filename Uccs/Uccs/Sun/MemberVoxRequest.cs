using System.Collections.Generic;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public class MemberVoxRequest : RdcRequest
	{
		public byte[]					Raw { get; set; }
		//public ChainTime				Time { get; set; }
		//public byte[]					Signature { get; set; }

		public override bool			WaitResponse => false;

		//public AccountAddress			Account;

		public MemberVoxRequest()
		{
		}


		public override RdcResponse Execute(Sun sun)
		{

///			if(Account == null)
///			{
///				Account = sun.Zone.Cryptography.AccountFrom(Signature, Hashify(sun.Zone));
///			}

///			lock(sun.Lock)
///			{
///				var m = sun.Members.Find(i => i.Generator == Account);
///
///				if(m == null)
///				{
///					if(sun.Settings.Roles.HasFlag(Role.Base))
///					{
///						if(sun.Synchronization == Synchronization.Synchronized && !sun.Database.LastConfirmedRound.Members.Any(i => i.Generator == Account))
///							return null;
///					}
///
///					m = new Member{Generator = Account};
///					sun.Members.Add(m)	;
///				}
///
///				if(IPs.Any())
///				{
///					if(!m.IPs.SequenceEqual(IPs))
///					{
///						m.IPs = IPs.ToArray();
///		
///						//foreach(var i in sun.Connections.Where(i => i != Peer))
///						//	i.Send(new GeneratorOnlineBroadcastRequest {Account = Account, Time = Time, IPs = IPs, Signature = Signature});
///					}
///				}
///				else if(m.Proxy == null || m.OnlineSince < Time)
///				{
///					//foreach(var i in sun.Connections.Where(i => i != Peer))
///					//	i.Send(new GeneratorOnlineBroadcastRequest {Account = Account, Time = Time, IPs = IPs, Signature = Signature});
///				
///					m.Proxy = Peer;
///					m.OnlineSince = Time;
///				}
///
///				return null;
///			}

			var s = new MemoryStream(Raw);
			var br = new BinaryReader(s);

			var v = new Vote(sun.Mcv);
			v.RawForBroadcast = Raw;
			v.ReadForBroadcast(br);

			lock(sun.Lock)
			{
				if(!sun.Settings.Roles.HasFlag(Role.Base))	throw new RdcNodeException(RdcNodeError.NotBase);

				var accepted = sun.ProcessIncoming(v);

				if(sun.Synchronization == Synchronization.Synchronized)
				{
					var r = sun.Mcv.FindRound(v.RoundId);
					var _v = r.Votes.Find(i => i.Signature.SequenceEqual(v.Signature));

					if(_v != null)
					{
						if(accepted) /// for the new vote
						{
							var m = sun.Mcv.LastConfirmedRound.Members.Find(i => i.Account == v.Generator);
							
							if(m != null)
							{
								m.BaseIPs	= v.BaseIPs.ToArray();
								m.HubIPs	= v.HubIPs.ToArray();
								m.Proxy		= Peer;
							}
						}
						else if(_v.Peers != null && !_v.Peers.Contains(Peer)) /// for the existing vote
							_v.BroadcastConfirmed = true;
					}
				}

				if(accepted)
				{
					sun.Broadcast(v, Peer);
				}
			}

			//var accepted = new List<Vote>();
/*
			var accepted = false;

			lock(sun.Lock)
			{
				if(!sun.Settings.Roles.HasFlag(Role.Base))	throw new RdcNodeException(RdcNodeError.NotBase);

				if(sun.Synchronization == Synchronization.Null || sun.Synchronization == Synchronization.Downloading || sun.Synchronization == Synchronization.Synchronizing)
				{
 					var min = sun.SyncCache.Any() ? sun.SyncCache.Max(i => i.Key) - Database.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
 
					if(v.RoundId < min || (sun.SyncCache.ContainsKey(v.RoundId) && sun.SyncCache[v.RoundId].Blocks.Any(j => j.Signature.SequenceEqual(v.Signature))))
					{
					}
					else
					{ 
						accepted = true;

						Core.SyncRound r;
						
						if(!sun.SyncCache.TryGetValue(v.RoundId, out r))
						{
							r = sun.SyncCache[v.RoundId] = new();
						}

						r.Blocks.Add(v);

						foreach(var i in sun.SyncCache.Keys)
						{
							if(i < min)
							{
								sun.SyncCache.Remove(i);
							}
						}	
					}
				}
				else if(sun.Synchronization == Synchronization.Synchronized)
				{
					//var notolder = sun.Database.LastConfirmedRound.Id - Database.Pitch;
					//var notnewer = sun.Database.LastConfirmedRound.Id + Database.Pitch * 2;

					var d = sun.Database;

					if(v.RoundId <= d.LastConfirmedRound.Id || d.LastConfirmedRound.Id + Database.Pitch * 2 < v.RoundId)
					{
					}
					else
					{
						var r = sun.Database.GetRound(v.RoundId);
				
						var ep = r.Blocks.Find(i => i.Signature.SequenceEqual(v.Signature));

						if(ep == null)
						{
							//accepted.Add(v);
							//r.BlockPieces.Add(p);
							//
							//var ps = r.BlockPieces.Where(i => i.Generator == p.Generator && i.Try == p.Try).OrderBy(i => i.Index);
							//
							//if(ps.Count() == p.Total && ps.Zip(ps.Skip(1), (x, y) => x.Index + 1 == y.Index).All(x => x))
							//{
							//	var s = new MemoryStream();
							//	var w = new BinaryWriter(s);
							//
							//	foreach(var i in ps)
							//	{
							//		s.Write(i.Data);
							//	}
							//
							//	s.Position = 0;
							//	var rd = new BinaryReader(s);
				
								//var b = new Vote(sun.Database);
								//b.Generator = p.Generator;
								//b.ReadForBroadcast(rd);

								//if(b.Generator != p.Generator)
								//	continue;

								///sun.Workflow?.Log.Report(this, "Block received ", $"{Hex.ToHexString(b.Generator.Prefix)}-{b.RoundId}");
				
							//}
							accepted = true;

							sun.ProcessIncoming(new[] {v});

							var m = sun.Database.LastConfirmedRound.Generators.Find(i => i.Account == v.Generator);
		
							if(m != null)
							{
								m.IPs = v.BaseIPs.ToArray();
								m.Proxy = Peer;
							}
						}
						else if(ep.Peers != null && !ep.Peers.Contains(Peer))
							ep.BroadcastConfirmed = true;
					}
				}

				if(accepted)
				{
					var rq = new GeneratorVoxRequest(v);
	
					foreach(var i in sun.Bases.Where(i => i != Peer))
					{
						i.Send(new GeneratorVoxRequest {Vote = rq.Vote});
					}
				}
			}*/

			return null; 
		}
				
//		public byte[] Hashify(Zone zone)
//		{
//			var s = new MemoryStream();
//			var w = new BinaryWriter(s);
//			//writer.Write(Generator);
//
//			//w.Write(Time);
//			w.Write(IPs, i => w.Write(i));
//
//			return zone.Cryptography.Hash(s.ToArray());
//		}
//		
//		public void Write(BinaryWriter writer)
//		{
//			writer.Write(Blocks, i => i.WriteForBroadcast(writer));
//		}
//		
//		public void Read(BinaryReader reader, Zone zone)
//		{
//			Blocks	= reader.Read<Vote>(v => v.ReadForBroadcast(reader));
//		}
//		
//		public void Sign(Zone zone, AccountKey generator)
//		{
//			Account = generator;
//			Signature = zone.Cryptography.Sign(generator, Hashify(zone));
//		}
	}
}
