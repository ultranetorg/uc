using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class BlocksBroadcastRequest : RdcRequest
	{
		public IEnumerable<BlockPiece>	Pieces { get; set; }
		public override bool			WaitResponse => false;

		public override RdcResponse Execute(Core core)
		{
			var accepted = new List<BlockPiece>();

			lock(core.Lock)
			{
				if(!core.Settings.Roles.HasFlag(Role.Base))	throw new RdcNodeException(RdcNodeError.NotBase);

				if(core.Synchronization == Synchronization.Null || core.Synchronization == Synchronization.Downloading || core.Synchronization == Synchronization.Synchronizing)
				{
 					var min = core.SyncCache.Any() ? core.SyncCache.Max(i => i.Key) - Database.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
 
					foreach(var i in Pieces)
					{
						if(i.RoundId < min || (core.SyncCache.ContainsKey(i.RoundId) && core.SyncCache[i.RoundId].Blocks.Contains(i)))
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

					var good = Pieces.Where(p => { 


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

					foreach(var p in good)
					{
						var r = core.Database.GetRound(p.RoundId);
				
						var ep = r.BlockPieces.Find(i => i.Equals(p));

						if(ep == null)
						{
							accepted.Add(p);
							r.BlockPieces.Add(p);
				
							var ps = r.BlockPieces.Where(i => i.Generator == p.Generator && i.Try == p.Try).OrderBy(i => i.Index);
			
							if(ps.Count() == p.Total && ps.Zip(ps.Skip(1), (x, y) => x.Index + 1 == y.Index).All(x => x))
							{
								var s = new MemoryStream();
								var w = new BinaryWriter(s);
				
								foreach(var i in ps)
								{
									s.Write(i.Data);
								}
				
								s.Position = 0;
								var rd = new BinaryReader(s);
				
								var b = Block.FromType(core.Database, p.Type);
								b.Generator = p.Generator;
								b.ReadForPiece(rd);

								//if(b.Generator != p.Generator)
								//	continue;
				
								core.ProcessIncoming(new Block[] {b});
							}
						}
						else
							if(ep.Peers != null && !ep.Peers.Contains(Peer))
								ep.Broadcasted = true;
					}
				}

				if(accepted.Any())
				{
					foreach(var i in core.Connections.Where(i => i.BaseRank > 0 && i != Peer))
					{
						i.Send(new BlocksBroadcastRequest{Pieces = accepted});
					}
				}
			}

			return null; 
		}
	}
}
