using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class ReleaseHistoryRequest : RdcRequest
	{
		public RealizationAddress	Realization { get; set; }

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{
				if(!core.Settings.Roles.HasFlag(Role.Chain))				throw new RdcNodeException(RdcNodeError.NotChain);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);

				var db = core.Database;
				
				return new ReleaseHistoryResponse {Releases = db.Releases.Where(Realization.Product.Author, 
																				Realization.Product.Name, 
																				i => i.Address.Platform == Realization.Platform, 
																				db.LastConfirmedRound.Id).ToArray()};
			}
		}
	}
		
	public class ReleaseHistoryResponse : RdcResponse
	{
		public IEnumerable<Release> Releases { get; set; }
	}
}
