using System.Net;
using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;
using Uccs.Vault;

namespace Uccs.Nexus;

public class NodeDeclaration
{
	public string		ApiLocalAddress { get; set; }
	public string		Net;

	public override string ToString()
	{
		return Net;
	}
}

public class Nexus
{
	public delegate void			Delegate(Nexus d);

	Flow							Flow;
	IClock							Clock;
	public NexusSettings			Settings;
	public List<NodeDeclaration>	Nodes = [];
	internal NexusApiServer			ApiServer;
	public static HttpClient		ApiHttpClient;
	public RdnNode					RdnNode;
	public PackageHub				PackageHub;
	public Vault.Vault				Vault;
	public byte[]					VaultAdminKey;
	public Delegate					Stopped;
	VoidDelegate					OpenIam;

	public NodeDeclaration			Find(string net) => Nodes.Find(i => i.Net == net);

	static Nexus()
	{
  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}

	public Nexus(NetBoot boot, NexusSettings settings, VaultSettings vaultsettings, IClock clock, Flow flow)
	{
		Settings = settings ?? new NexusSettings(boot);
		Settings.Packages = Settings.Packages ?? Path.Join(boot.Profile, "Packages");
		Clock = clock;
		Flow = flow;

		new FileLog(Flow.Log, Flow.Name, Settings.Profile);

		if(Directory.Exists(Settings.Profile))
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, $"{GetType().Name}.{Cli.FailureExt}"))
				File.Delete(i);

		Vault = new Vault.Vault(boot.Profile, boot.Zone, vaultsettings, flow);		

		if(Settings.Api != null)
		{
			RunApi();
		}
	}

	public override string ToString()
	{
		return string.Join(" - ", new string[]{	Settings.Name,
												ApiServer != null ? "A" : null,
												///Rdn?.ToString()
												}.Where(i => i != null));
	}

	public void Stop()
	{
		Flow.Abort();

		Stopped?.Invoke(this);


		foreach(var i in Nodes.ToArray())
		{	
			//i.Node.Stop();
			Nodes.Remove(i);
		}

		RdnNode?.Stop();
		Vault.Stop();
		ApiServer?.Stop();
	}
	
	public void RunRdn(RdnNodeSettings rdnsettings)
	{
		RdnNode		= new RdnNode(Settings.Name, Settings.Zone, Settings.Profile, rdnsettings, Clock, Flow);
		PackageHub	= new PackageHub(RdnNode, Settings.Packages);
		
		Nodes = [new NodeDeclaration {Net = Rdn.Rdn.Root}];
	}

	public void RunApi()
	{
		if(!HttpListener.IsSupported)
		{
			Environment.ExitCode = -1;
			throw new RequirementException("Windows XP SP2, Windows Server 2003 or higher is required to use the application.");
		}

		if(ApiServer != null)
			throw new NodeException(NodeError.AlreadyRunning);

		ApiServer = new NexusApiServer(this, Flow);
	
		//ApiStarted?.Invoke(this);
	}

	public McvApiClient GetMcvNodeApi(string net)
	{
		var ni = Find(net);

		return new McvApiClient(ni.ApiLocalAddress, null, ApiHttpClient);
	}

	public void SetupApplicationEnvironemnt(Ura address)
	{
		Environment.SetEnvironmentVariable(Application.ProfileKey,			Settings.Profile);
		Environment.SetEnvironmentVariable(Application.PackageAddressKey,	address.ToString());

		Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, address);
	}
}	
