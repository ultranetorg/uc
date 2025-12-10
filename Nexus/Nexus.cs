using System.Diagnostics;
using System.Net;
using Uccs.Rdn;
using Uccs.Vault;

namespace Uccs.Nexus;

public class Nexus : IProgram
{
	public delegate void			Delegate(Nexus d);

	public Flow						Flow;
	IClock							Clock;
	public NexusSettings			Settings;
	internal NexusApiServer			ApiServer;
	public static HttpClient		ApiHttpClient;
	public RdnNode					RdnNode;
	public PackageHub				PackageHub;
	public Vault.Vault				Vault;
	public byte[]					VaultAdminKey;
	public Delegate					Stopped;
	VoidDelegate					OpenIam;

	public NnpTcpPeering			NnpPeering;
	public NnpIppServer				NnpIppServer;
	public NnpIppClientConnection	NnpConnection;

	static Nexus()
	{
  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}

	public Nexus(NetBoot boot, NexusSettings settings, VaultSettings vaultsettings, IClock clock, Flow flow)
	{
		Settings = settings ?? new NexusSettings(boot.Zone, boot.Profile);
		Settings.Packages = Settings.Packages ?? Path.Join(boot.Profile, "Packages");
		Clock = clock;
		Flow = flow;

		new FileLog(Flow.Log, Flow.Name, Settings.Profile);

		if(Directory.Exists(Settings.Profile))
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, $"{GetType().Name}.{Cli.FailureExt}"))
				File.Delete(i);

		Vault = new Vault.Vault(boot.Profile, boot.Zone, vaultsettings, flow);		

		if(Settings.NnPeering != null)
		{
			NnpPeering = new NnpTcpPeering(this, Settings.Name, Settings.NnPeering, 0, flow);
			NnpIppServer = new NnpIppServer(this);
			NnpConnection = new NnpIppClientConnection(this, NnpIppConnection.GetName(Settings.Host), flow);

			NnpPeering.Run();
		}

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

		NnpConnection?.Disconnect();
		NnpPeering?.Stop();
		RdnNode?.Stop();
		Vault.Stop();
		ApiServer?.Stop();
	}

	public Thread CreateThread(Action action)
	{
		return new Thread(() => { 
									try
									{
										action();
									}
									catch(OperationCanceledException)
									{
									}
									catch(Exception ex) when (!Debugger.IsAttached)
									{
										if(Flow.Active)
										{
											File.WriteAllText(Path.Join(Settings.Profile, "Abort." + Cli.FailureExt), ex.ToString());
											Flow.Log?.ReportError(this, "Abort", ex);
										}
									}
								});
	}
	
	public void RunRdn(RdnNodeSettings rdnsettings)
	{
		RdnNode		= new RdnNode(Settings.Name, Settings.Zone, Settings.Profile, Settings, rdnsettings, Clock, Flow);
		PackageHub	= new PackageHub(RdnNode, Settings.Packages);
		
		///Nodes = [new NodeDeclaration {Net = Rdn.Rdn.Root, ApiLocalAddress = RdnNode.Settings.Api.LocalAddress(RdnNode.Net)}];
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

//	public NnApiClient GetNetToNetnNodeApi(string net)
//	{
//		var d = Find(net);
//
//		if(d == null)
//			throw new NexusException("No node available for this net");
//
//		return new NnApiClient(d.ApiLocalAddress, http: ApiHttpClient);
//	}

	public void SetupApplicationEnvironemnt(Ura address)
	{
		Environment.SetEnvironmentVariable(Application.ProfileKey,			Settings.Profile);
		Environment.SetEnvironmentVariable(Application.PackageAddressKey,	address.ToString());

		Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, address);
	}
}	
