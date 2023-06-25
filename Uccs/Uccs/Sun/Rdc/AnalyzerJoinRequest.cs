using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AnalyzerJoinRequest : RdcRequest
	{
		public int				RoundId { get; set; }
		public byte[]			Signature { get; set; }

		public AccountAddress	Account;
		public override bool	WaitResponse => false;

		public AnalyzerJoinRequest()
		{
		}

		public override string ToString()
		{
			return base.ToString() + ", Account=" + Account;
		}

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Synchronization == Synchronization.Null || core.Synchronization == Synchronization.Downloading || core.Synchronization == Synchronization.Synchronizing)
				{
	 				var min = core.SyncCache.Any() ? core.SyncCache.Max(i => i.Key) - Database.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
	 
					if(RoundId < min || (core.SyncCache.ContainsKey(RoundId) && core.SyncCache[RoundId].AnalyzerJoins.Any(i => i.Account == Account)))
					{
						return null;
					}
	
					Core.SyncRound r;
							
					if(!core.SyncCache.TryGetValue(RoundId, out r))
					{
						r = core.SyncCache[RoundId] = new();
					}
	
					r.AnalyzerJoins.Add(this);
					
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
						if(d.FindRound(i) is Round r && r.AnalyzerJoinRequests.Any(j => j.Account == Account))
							return null;
				
					core.Database.GetRound(RoundId).AnalyzerJoinRequests.Add(this);
				}
	
				foreach(var i in core.Connections.Where(i => i != Peer))
				{
					i.Send(new AnalyzerJoinRequest {RoundId = RoundId, Signature = Signature});
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
			//IPs		= reader.ReadArray(() => reader.ReadIPAddress());
			Signature	= reader.ReadSignature();
		
			Account = zone.Cryptography.AccountFrom(Signature, Hashify(zone));
		}
	}
}
