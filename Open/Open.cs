using System.Diagnostics;
using System.Text;
using Uccs.Net;
using Uccs.Nexus;

namespace Uccs.Open;

public class Open : Cli
{
	public static HttpClient	ApiHttpClient;
	public NexusSettings		Settings;

	RdnApiClient				_Rdn;
	public RdnApiClient			RdnApi => _Rdn ??= new RdnApiClient(Settings.Api.LocalAddress(Rdn.Rdn.ByZone(Settings.Zone)), null, ApiHttpClient);

	NexusApiClient				_Nexus;
	public NexusApiClient		NexusApi => _Nexus ??= new NexusApiClient(Settings.Api.LocalAddress(Settings.Zone, Api.Nexus), null, ApiHttpClient);

	static Open()
	{
  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}

	static void Main(string[] args)
	{
		var boot = new NetBoot(ExeDirectory);
		var s = new NexusSettings(boot.Zone, boot.Profile) {Name = Guid.NewGuid().ToString()};
		var u = new Open(s, new Flow(nameof(Open), new Log()));

		u.Execute(boot);

		u.Flow.Abort();
	}

	public Open(NexusSettings settings, Flow flow)
	{
		Settings = settings;
		Flow = flow;

		foreach(var i in Directory.EnumerateFiles(settings.Profile, $"{GetType().Name}.{FailureExt}"))
			File.Delete(i);
	}

	public override OpenCommand Create(IEnumerable<Xon> commnad, Flow flow)
	{
		var args = commnad.ToList();

		return new OpenCommand(this, args, flow);
	}

	public void Start(Unel address, Flow flow)
	{
		if(address.Net == Net.Net.Root || address.Net is null)
		{
			var ura = Ura.Parse(address.ToString());

			var d = RdnApi.FindLocalResource(ura, flow)?.Last
					?? 
					RdnApi.Call<ResourcePpr>(new PpcApc {Request = new ResourcePpc {Identifier = new (ura)}}, flow)?.Resource?.Data;
	
			if(d == null)
				throw new OpenException("Incorrect resource type");
	
			//Ura apr = null;
			Ura aprv = null;
	
			if(d.Type.Content == ContentType.Rdn_ProductManifest)
			{
				var lrr = RdnApi.Download(ura, flow);
	
				var m = ProductManifest.FromXon(new Xon(new StreamReader(new MemoryStream(RdnApi.Call<byte[]>(new LocalReleaseReadApc {Address = lrr.Address, Path=""}, flow)), Encoding.UTF8).ReadToEnd()));
	
				aprv = m.Realizations.FirstOrDefault(i => i.Condition.Match(Platform.Current)).Latest;
			}
			else if(d.Type.Content == ContentType.Rdn_VersionManifest)
			{
				aprv = ura;
			}
			else
				throw new OpenException("Incorrect resource type");
	
			NexusApi.DeployPackage(aprv, Settings.Packages, flow);
	
	 		var vmpath = Directory.EnumerateFiles(PackageHub.AddressToDeployment(Settings.Packages, aprv), "*." + VersionManifest.Extension).First();
	 
	 		var vm = VersionManifest.Load(vmpath);
	 
			var exe = vm.MatchExecution(Platform.Current);
	
			SetupApplicationEnvironemnt(aprv);
	
	 		var ps = new Process();
	 		ps.StartInfo.UseShellExecute = true;
	 		ps.StartInfo.FileName = Path.Join(PackageHub.AddressToDeployment(Settings.Packages, aprv), exe.Path);
	 		ps.StartInfo.Arguments = exe.Arguments;
	
	 		ps.Start();
		}
		else
		{
			///if(Find(address.Net) == null)
			///{
			///	ConnectNetwork(address.Net);
			///}
			///
			///GetMcvNodeApi(address.Net).Send(new StartApc {Entity = address.Entity}, flow);
		}
	}

	public void SetupApplicationEnvironemnt(Ura address)
	{
		Environment.SetEnvironmentVariable(Application.PackageAddressKey,	address.ToString());
		Environment.SetEnvironmentVariable(Application.ProfileKey,			Settings.Profile);

		Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, address);
	}
}	
