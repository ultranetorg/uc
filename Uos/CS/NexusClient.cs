using Uccs.Net;

namespace Uccs.Uos
{
	public class NexusClient
	{
		public string			ProductsPath;	
		public string			SunAddress; 		
		public string			SunApiKey;
		//public McvZone			Net;
		//
		//public ResourceHub		ResourceHub;
		//public PackageHub		PackageHub;
		public UosApiClient		Uos;
		public RdnApiClient		Rdn;
		HttpClient				Http = new HttpClient();


		public NexusClient()
		{
			var hch = new HttpClientHandler();
			hch.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
			var http = new HttpClient(hch) {Timeout = Timeout.InfiniteTimeSpan};

			//ProductsPath	=	Environment.GetEnvironmentVariable(Nexus.BootProductsPath);
			//SunAddress	= 	Environment.GetEnvironmentVariable(Nexus.BootSunAddress);
			//SunApiKey		=	Environment.GetEnvironmentVariable(Nexus.BootSunApiKey);
			//Net			=	Environment.GetEnvironmentVariable(Nexus.BootZone);

			//Sun = new SunJsonApiClient(Http, SunAddress, SunApiKey);
			//
			//var s = Sun.Request<SettingsResponse>(new SettingsApc(), new Flow("GetSettings"));

			//ResourceHub = new ResourceHub(null, Net, Path.Join(s.ProfilePath, nameof(ResourceHub)));
			//PackageHub = new PackageHub(null, ProductsPath);

			Uos = new UosApiClient(http, Environment.GetEnvironmentVariable(Application.ApiAddressEnvKey), Environment.GetEnvironmentVariable(Application.ApiKeyEnvKey));

			var s = Uos.Request<NodeInstance>(new NodeInfoApc { Mcvid = Uccs.Rdn.Rdn.Local.Id}, new Flow(GetType().Name));

			Rdn = new RdnApiClient(http, s.Api.ListenAddress, s.Api.AccessKey);
		}

		public PackageInfo GetPackage(AprvAddress package, Flow flow)
		{
			var p = Rdn.Request<PackageInfo>(new LocalPackageApc {Address = package}, flow);

			if(p == null)
			{
				Uos.Send(new PackageDeployApc {Address = package}, flow);

				return Rdn.Request<PackageInfo>(new LocalPackageApc {Address = package}, flow);
			}

			return p;
		}

		public void Start(Uri address, Flow flow)
		{
			///var s = Sun.Request<PackageDownloadReport>(new PackageDownloadReportCall {Package = PackageAddress.Parse(address.LocalPath)}, workflow);
			///
			///if(s.Manifest == null)
			///{
			///	Sun.Send(new InstallPackageCall {Package = PackageAddress.Parse(address.LocalPath)}, workflow);
			///
			///	while(true)
			///	{
			///		workflow.Wait(1);
			///
			///		s = Sun.Request<PackageDownloadReport>(new PackageDownloadReportCall {Package = PackageAddress.Parse(address.LocalPath)}, workflow);
			///
			///		if(s.Download == null)
			///		{
			///			Execute(address);
			///			return;
			///		}
			///	}
			///}
		}

		void Execute(Uri request)
		{
			/// var r = PackageAddress.Parse(request.LocalPath);
			/// 
			/// var f = Directory.EnumerateFiles(PackageHub.AddressToPath(r), "*.start").FirstOrDefault();
			/// 
			/// if(f != null)
			/// {
			/// 	string setenv(PackageAddress a, string p)
			/// 	{
			/// 		p += ";" + PackageHub.AddressToPath(a);
			/// 
			/// 		foreach(var i in PackageHub.Find(a).Manifest.CompleteDependencies.Where(i => i.Type == DependencyType.Critical && i.Flags.HasFlag(DependencyFlag.SideBySide)))
			/// 		{
			/// 			p += ";" + setenv(i.Package, p);
			/// 		}
			/// 
			/// 		return p;
			/// 	}
			/// 
			/// 	Environment.SetEnvironmentVariable("PATH", setenv(r, Environment.GetEnvironmentVariable("PATH")));
			/// 	
			/// 	var s = new Xon(File.ReadAllText(f));
			/// 	
			/// 	Process.Start(s.Get<string>("Executable"), s.Get<string>("Arguments"));
			/// }
		}
	}
}
