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
		public Account							Generator {get; set;} /// serializable
		
		public int								ApiFailures;
		public int								ApiReachFailures;

 		public abstract Rp						Request<Rp>(Request rq) where Rp : class;

		public NextRoundResponse				Send(NextRoundRequest call) => Request<NextRoundResponse>(call);
		public LastOperationResponse			Send(LastOperationRequest call) => Request<LastOperationResponse>(call);
		public DelegateTransactionsResponse		Send(DelegateTransactionsRequest call) => Request<DelegateTransactionsResponse>(call);
		public GetOperationStatusResponse		Send(GetOperationStatusRequest call) => Request<GetOperationStatusResponse>(call);
		public GetMembersResponse				Send(GetMembersRequest call) => Request<GetMembersResponse>(call);

 
		public AuthorInfoResponse				GetAuthorInfo(string author, bool confirmed) => Request<AuthorInfoResponse>(new AuthorInfoRequest{ Name = author, Confirmed = confirmed });
		public AccountInfoResponse				GetAccountInfo(Account account, bool confirmed) => Request<AccountInfoResponse>(new AccountInfoRequest{ Account = account, Confirmed = confirmed });
		public QueryReleaseResponse				QueryRelease(ReleaseQuery release, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = new [] { release }, Confirmed = confirmed });
		public LocatePackageResponse			LocatePackage(PackageAddress package) => Request<LocatePackageResponse>(new LocatePackageRequest{ Package = package });
		public DownloadPackagerResponse			DownloadPackage(PackageDownload request) => Request<DownloadPackagerResponse>(new DownloadPackageRequest{ Request = request });
	}
}
