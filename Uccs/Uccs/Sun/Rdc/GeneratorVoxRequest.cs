using System.Collections.Generic;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public class GeneratorVoxRequest : RdcRequest
	{
		public byte[]					Votes { get; set; }
		//public ChainTime				Time { get; set; }
		//public byte[]					Signature { get; set; }

		public override bool			WaitResponse => false;

		//public AccountAddress			Account;

		public GeneratorVoxRequest()
		{
		}

		public GeneratorVoxRequest(IEnumerable<Vote> votes)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(votes, i => i.WriteForBroadcast(w));

			Votes = s.ToArray();
		}

		public override RdcResponse Execute(Core core)
		{
///			if(Account == null)
///			{
///				Account = core.Zone.Cryptography.AccountFrom(Signature, Hashify(core.Zone));
///			}

///			lock(core.Lock)
///			{
///				var m = core.Members.Find(i => i.Generator == Account);
///
///				if(m == null)
///				{
///					if(core.Settings.Roles.HasFlag(Role.Base))
///					{
///						if(core.Synchronization == Synchronization.Synchronized && !core.Database.LastConfirmedRound.Members.Any(i => i.Generator == Account))
///							return null;
///					}
///
///					m = new Member{Generator = Account};
///					core.Members.Add(m)	;
///				}
///
///				if(IPs.Any())
///				{
///					if(!m.IPs.SequenceEqual(IPs))
///					{
///						m.IPs = IPs.ToArray();
///		
///						//foreach(var i in core.Connections.Where(i => i != Peer))
///						//	i.Send(new GeneratorOnlineBroadcastRequest {Account = Account, Time = Time, IPs = IPs, Signature = Signature});
///					}
///				}
///				else if(m.Proxy == null || m.OnlineSince < Time)
///				{
///					//foreach(var i in core.Connections.Where(i => i != Peer))
///					//	i.Send(new GeneratorOnlineBroadcastRequest {Account = Account, Time = Time, IPs = IPs, Signature = Signature});
///				
///					m.Proxy = Peer;
///					m.OnlineSince = Time;
///				}
///
///				return null;
///			}
			var s = new MemoryStream(Votes);
			var br = new BinaryReader(s);

			var votes = br.Read<Vote>(() => new Vote(core.Database), v => v.ReadForBroadcast(br)).ToArray();

			var accepted = new List<Vote>();

			lock(core.Lock)
			{
				if(!core.Settings.Roles.HasFlag(Role.Base))	throw new RdcNodeException(RdcNodeError.NotBase);

				if(core.Synchronization == Synchronization.Null || core.Synchronization == Synchronization.Downloading || core.Synchronization == Synchronization.Synchronizing)
				{
 					var min = core.SyncCache.Any() ? core.SyncCache.Max(i => i.Key) - Database.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
 
					foreach(var i in votes)
					{
						if(i.RoundId < min || (core.SyncCache.ContainsKey(i.RoundId) && core.SyncCache[i.RoundId].Blocks.Any(j => j.Signature.SequenceEqual(i.Signature))))
						{
							continue;
						}

						Core.SyncRound l;
						
						if(!core.SyncCache.TryGetValue(i.RoundId, out l))
						{
							l = core.SyncCache[i.RoundId] = new();
						}

						l.Blocks.Add(i);
	 					accepted.Add(i);
 					}
					
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
					//var notolder = core.Database.LastConfirmedRound.Id - Database.Pitch;
					//var notnewer = core.Database.LastConfirmedRound.Id + Database.Pitch * 2;

					var d = core.Database;

					var good = votes.Where(p => { 


													//if(p.Type == BlockType.JoinMembersRequest)
													//{
													//	for(int i = p.RoundId; i > p.RoundId - Database.Pitch * 2; i--) /// not more than 1 request per [2 x Pitch] rounds
													//		if(d.FindRound(i) is Round r && r.JoinRequests.Any(j => j.Generator == p.Generator))
													//			return false;
													//}
													//else
													{
														if(p.RoundId <= d.LastConfirmedRound.Id || d.LastConfirmedRound.Id + Database.Pitch * 2 < p.RoundId)
															return false;
													}

													return true;
												}).ToArray();

					foreach(var v in good)
					{
						var r = core.Database.GetRound(v.RoundId);
				
						var ep = r.Blocks.Find(i => i.Signature.SequenceEqual(v.Signature));

						if(ep == null)
						{
							accepted.Add(v);
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
				
								//var b = new Vote(core.Database);
								//b.Generator = p.Generator;
								//b.ReadForBroadcast(rd);

								//if(b.Generator != p.Generator)
								//	continue;

								///core.Workflow?.Log.Report(this, "Block received ", $"{Hex.ToHexString(b.Generator.Prefix)}-{b.RoundId}");
				
							//}
						}
						else if(ep.Peers != null && !ep.Peers.Contains(Peer))
							ep.BroadcastConfirmed = true;
					}

					core.ProcessIncoming(accepted);

					foreach(var v in accepted)
					{
						var m = core.Database.LastConfirmedRound.Generators.Find(i => i.Account == v.Generator);
		
						if(m != null)
						{
							m.IPs = v.BaseIPs.ToArray();
							m.Proxy = Peer;
						}
					}
				}

				if(accepted.Any())
				{
					var vox = new GeneratorVoxRequest(accepted);

					foreach(var i in core.Bases.Where(i => i != Peer))
					{
						i.Send(new GeneratorVoxRequest {Votes = vox.Votes});
					}
				}
			}

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
