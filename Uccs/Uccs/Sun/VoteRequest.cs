using System.IO;
using System.Linq;
using Org.BouncyCastle.Pkix;

namespace Uccs.Net
{
	public class VoteRequest : RdcRequest
	{
		public byte[]					Raw { get; set; }

		public override bool			WaitResponse => false;

		public VoteRequest()
		{
		}


		public override RdcResponse Execute(Sun sun)
		{
			if(!sun.Roles.HasFlag(Role.Base))
				throw new RdcNodeException(RdcNodeError.NotBase);

			var s = new MemoryStream(Raw);
			var br = new BinaryReader(s);

			var v = new Vote(sun.Mcv);
			v.RawForBroadcast = Raw;
			v.ReadForBroadcast(br);

			lock(sun.Lock)
			{
				sun.Statistics.Consensing.Begin();
				
				var accepted = false;

				try
				{
					accepted = sun.ProcessIncoming(v, false);
				}
				catch(ConfirmationException ex)
				{
					sun.ProcessConfirmationException(ex);
					accepted = true; /// consensus failed but the vote looks valid
				}
								
				sun.Statistics.Consensing.End();

				if(sun.Synchronization == Synchronization.Synchronized)
				{
					//var r = sun.Mcv.FindRound(v.RoundId);
					var _v = v.Round?.Votes.Find(i => i.Signature.SequenceEqual(v.Signature)); 

					if(_v != null) /// added or existed
					{
						if(accepted) /// for the new vote
						{
							var m = sun.Mcv.LastConfirmedRound.Members.Find(i => i.Account == v.Generator);
							
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
					sun.Broadcast(v, Peer);
					sun.Statistics.AccpetedVotes++;
				}
				else
					sun.Statistics.RejectedVotes++;

			}

			return null; 
		}
	}
}
