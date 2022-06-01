using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public abstract class RpcClient
	{
		public int								ApiFailures;
		public int								ApiReachFailures;
		public Account							Generator {get; set;} /// serializable

 		public abstract Rp						Request<Rp>(Request rq) where Rp : class;

		public NextRoundResponse				Send(NextRoundRequest call) => Request<NextRoundResponse>(call);
		public LastOperationResponse			Send(LastOperationRequest call) => Request<LastOperationResponse>(call);
		public DelegateTransactionsResponse		Send(DelegateTransactionsRequest call) => Request<DelegateTransactionsResponse>(call);
		public GetOperationStatusResponse		Send(GetOperationStatusRequest call) => Request<GetOperationStatusResponse>(call);
		public GetMembersResponse				Send(GetMembersRequest call) => Request<GetMembersResponse>(call);
	}
}
