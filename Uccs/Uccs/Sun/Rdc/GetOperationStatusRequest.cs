using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class GetOperationStatusRequest : RdcRequest
	{
		public IEnumerable<OperationAddress>	Operations { get; set; }

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Roles.HasFlag(Role.Chain))				throw new RdcNodeException(RdcNodeError.NotChain);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);
	
				return	new GetOperationStatusResponse
						{
							Operations = Operations.Select(o => new {	A = o,
																		O = core.Transactions.Where(t => t.Signer == o.Account && t.Operations.Any(i => i.Id == o.Id))
																							 .SelectMany(t => t.Operations)
																							 .FirstOrDefault(i => i.Id == o.Id)
																		?? 
																		core.Database.Accounts.FindLastOperation(o.Account, i => i.Id == o.Id)})
													.Select(i => new GetOperationStatusResponse.Item {	Account		= i.A.Account,
																										Id			= i.A.Id,
																										Placing		= i.O == null ? PlacingStage.FailedOrNotFound : i.O.Placing}).ToArray()
						};
			}
		}
	}
	
	public class GetOperationStatusResponse : RdcResponse
	{
		public class Item
		{
			public AccountAddress	Account { get; set; }
			public int				Id { get; set; }
			public PlacingStage		Placing { get; set; }
		}

		public IEnumerable<Item> Operations { get; set; }
	}
}
