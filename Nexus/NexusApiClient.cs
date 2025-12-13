using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class NexusApiClient : ApiClient
{
	public PackageInfo FindLocalPackage(Ura address, Flow flow) => Call<PackageInfo>(new LocalPackageApc { Address = address }, flow);

	public NexusApiClient(string address, string accesskey, HttpClient http = null, int timeout = 30) : base(address, accesskey, http, timeout)
	{
	}

	public PackageInfo DeployPackage(Ura address, string desination, Flow flow)
	{
		Send(new PackageDeployApc { Address = address, DeploymentPath = desination }, flow);

		do
		{
			var d = Call<PackageActivityProgress>(new PackageActivityProgressApc { Package = address }, flow);

			if(d is null)
			{
				return Call<PackageInfo>(new LocalPackageApc { Address = address }, flow);

				//if(lrr.Availability == Availability.Full)
				//{
				//	return lrr;
				//}
				//else
				//{
				//	throw new ResourceException(ResourceError.);
				//}
			}

			Thread.Sleep(100);
		}
		while(flow.Active);

		throw new OperationCanceledException();
	}
}

//public class NexusPropertyApc : Apc
//{
//	public string Path { get; set; }
//}
//
//
//internal class NodeInfoApc : Apc
//{
//	public string Net { get; set; }
//}
