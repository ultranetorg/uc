using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class VoteRequest : PeerCall<PeerResponse>
	{
		public byte[]				Raw { get; set; }
		public override bool		WaitResponse => false;

		public VoteRequest()
		{
		}
		
		public override PeerResponse Execute()
		{
			if(!Mcv.Roles.HasFlag(Role.Base))
				throw new NodeException(NodeError.NotBase);

			var s = new MemoryStream(Raw);
			var br = new BinaryReader(s);

			var v = Mcv.CreateVote();
			v.RawForBroadcast = Raw;
			v.ReadForBroadcast(br);

			lock(Mcv.Lock)
			{
				Sun.Statistics.Consensing.Begin();
				
				var accepted = false;

				try
				{
					accepted = Mcv.ProcessIncoming(v, false);
				}
				catch(ConfirmationException ex)
				{
					Mcv.ProcessConfirmationException(ex);
					accepted = true; /// consensus failed but the vote looks valid
				}
								
				Sun.Statistics.Consensing.End();

				if(Mcv.Synchronization == Synchronization.Synchronized)
				{
					//var r = sun.Mcv.FindRound(v.RoundId);
					var _v = v.Round?.Votes.Find(i => i.Signature.SequenceEqual(v.Signature)); 

					if(_v != null) /// added or existed
					{
						if(accepted) /// for the new vote
						{
							var m = Mcv.LastConfirmedRound.Members.Find(i => i.Account == v.Generator);
							
							if(m != null)
							{
								//m.BaseRdcIPs	= v.BaseRdcIPs.ToArray();
								//m.SeedHubRdcIPs	= v.SeedHubRdcIPs.ToArray();
								m.Proxy			= Peer;
							}
						}
						else if(_v.Peers != null && !_v.Peers.Contains(Peer)) /// for the existing vote
							_v.BroadcastConfirmed = true;
					}
				}

				if(accepted)
				{
					Sun.Broadcast(Mcv, v, Peer);
					Sun.Statistics.AccpetedVotes++;
				}
				else
					Sun.Statistics.RejectedVotes++;

			}

			return null; 
		}
	}
}
