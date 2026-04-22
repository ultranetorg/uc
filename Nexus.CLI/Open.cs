using System.Diagnostics;
using System.Text;
using Uccs.Net;
using Uccs.Nexus;
using Uccs.Rdn;

namespace Uccs.Nexus.CLI;

public class Open : Cli
{
	public static HttpClient	ApiHttpClient;
	public NexusSettings		Settings;

	RdnApiClient				_Rdn;
	public RdnApiClient			RdnApi => _Rdn ??= new RdnApiClient(Settings.Api.LocalNodeAddress(Rdn.Rdn.ByZone(Settings.Zone)), null, ApiHttpClient);

	NexusApiClient				_Nexus;
	public NexusApiClient		NexusApi => _Nexus ??= new NexusApiClient(Settings.Api.LocalSystemAddress(Settings.Zone, Api.Nexus), null, ApiHttpClient);

	static Open()
	{
  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}


	public void Start(Snp address, Flow flow)
	{
//		if(address.Net == Net.Net.Root || address.Net is null)
//		{
//			var ura = Ura.Parse(address.ToString());
//
//			var d = RdnApi.FindLocalResource(ura, flow)?.Last
//					?? 
//					RdnApi.Call<ResourcePpr>(new PpcApc {Request = new ResourcePpc {Identifier = new (ura)}}, flow)?.Resource?.Data;
//	
//			if(d == null)
//				throw new OpenException("Incorrect resource type");
//	
//			//Ura apr = null;
//			Ura aprv = null;
//	
//			if(d.Type.Content == ContentType.Package_ProductManifest)
//			{
//				var lrr = RdnApi.Download(ura, flow);
//	
//				var m = ProductManifest.FromXon(new Xon(new StreamReader(new MemoryStream(RdnApi.Call<byte[]>(new LocalReleaseReadApc {Address = lrr.Address, Path=""}, flow)), Encoding.UTF8).ReadToEnd()));
//	
//				aprv = m.Realizations.FirstOrDefault(i => i.Condition.Match(Platform.Current)).Latest;
//			}
//			else if(d.Type.Content == ContentType.Package_VersionManifest)
//			{
//				aprv = ura;
//			}
//			else
//				throw new OpenException("Incorrect resource type");
//	
//			NexusApi.DeployPackage(aprv, Settings.Packages, flow);
//	
//	 		var vmpath = Directory.EnumerateFiles(PackageHub.AddressToDeployment(Settings.Packages, aprv), "*." + PackageManifest.Extension).First();
//	 
//	 		var vm = PackageManifest.Load(vmpath);
//	 
//			var exe = vm.MatchExecution(Platform.Current);
//	
//			SetupApplicationEnvironemnt(aprv);
//	
//	 		var ps = new Process();
//	 		ps.StartInfo.UseShellExecute = true;
//	 		ps.StartInfo.FileName = Path.Join(PackageHub.AddressToDeployment(Settings.Packages, aprv), exe.Path);
//	 		ps.StartInfo.Arguments = exe.Arguments;
//	
//	 		ps.Start();
//		}
//		else
//		{
//			///if(Find(address.Net) == null)
//			///{
//			///	ConnectNetwork(address.Net);
//			///}
//			///
//			///GetMcvNodeApi(address.Net).Send(new StartApc {Entity = address.Entity}, flow);
//		}
	}

	public void SetupApplicationEnvironemnt(Ura address)
	{
		Environment.SetEnvironmentVariable(Uccs.Nexus.Application.PackageAddressKey,	address.ToString());
		Environment.SetEnvironmentVariable(Uccs.Nexus.Application.ProfileKey,		Settings.Profile);

		Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, address);
	}
}	
