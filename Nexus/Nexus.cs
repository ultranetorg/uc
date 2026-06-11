using System.Diagnostics;
using System.Net;
using System.Text;
using Uccs.Rdn;
using Uccs.Vault;

namespace Uccs.Nexus;

public class Nexus : IProgram
{
	public Flow						Flow;
	public NexusSettings			Settings;
	internal NexusApiServer			ApiServer;
	public static HttpClient		ApiHttpClient;
	public RdnNode					RdnNode;
	public PackageHub				PackageHub;
	public Vault.Vault				Vault;
	public byte[]					VaultAdminKey;
	public Delegate					Stopped;

	public IccpPeering				IccpPeering;
	public IccpLcpServer			IccpLcpServer;
	
	public delegate void			Delegate(Nexus d);

	public Nexus(NetBoot boot, NexusSettings settings, VaultSettings vaultsettings, Flow flow)
	{
		Settings = settings;
		Flow = flow;

		var mutex = new Mutex(false, $@"Global\Uos.{GetType().Name}.{boot.Profile.Replace(Path.DirectorySeparatorChar, '_').ToLower()}");

		if(!mutex.WaitOne(0, false))
			throw new Exception("Another instance is already running");

		new FileLog(Flow.Log, GetType().Name, Settings.Profile, flow);

		if(Directory.Exists(Settings.Profile))
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, $"{GetType().Name}.{Cli.FailureExt}"))
				File.Delete(i);

		Vault = new Vault.Vault(boot.Profile, boot.Zone, vaultsettings, flow);		

		if(Settings.IccpPeering != null)
		{
			IccpLcpServer = new IccpLcpServer(this);
	
			IccpPeering = new IccpPeering(	this, 
											Settings.Name, 
											Settings.IccpPeering, 
											IccpLcpServer, 
											() => IccpLcpServer.Locals.Select(i => i.Net).ToList(), 
											() => RdnNode.Peering.Call(new MembersPpc(), flow).Members.SelectMany(i => i.GraphPpiEndpoints.Select(i => i.IP)).ToArray(), 
											Flow);
			IccpPeering.Run();
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

		ApiServer?.Stop();
		RdnNode?.Stop();
		IccpPeering?.Stop();
		Vault.Stop();

		Stopped?.Invoke(this);
	}
	
	public void RunRdn(RdnNodeSettings rdnsettings, IClock clock)
	{
		var d = Path.Join(Settings.Profile, Rdn.Rdn.ByZone(Settings.Zone).Address);
		Directory.CreateDirectory(d);
		
		RdnNode		= new RdnNode(Settings.Zone, d, Settings, rdnsettings, clock, Flow.CreateNested(new Log(), d));
		PackageHub	= new PackageHub(RdnNode, Settings.Packages);

		if(IccpPeering != null)
		{
			IccpPeering.Mcv = RdnNode.Mcv;
		}
		///Nodes = [new NodeDeclaration {Net = Rdn.Rdn.Root, ApiLocalAddress = RdnNode.Settings.Api.LocalAddress(RdnNode.Net)}];
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

	public IccpLcpClientConnection CreateIccpClientConnection()
	{
		var c = new	IccpLcpClientConnection(this, IccpLcpConnection.GetName(Settings.Host), Flow);
		return c;
	}

	//public IccpLcpClientConnection CreateIccpClientConnection(Constructor constructor)
	//{
	//	var c = new	IccpLcpClientConnection(this, IccpLcpConnection.GetName(Settings.Host), Flow);
	//	c.Constructor.Merge(constructor);
	//	return c;
	//}

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

	public DeployedNode RunNode(string net, Flow flow)
	{
		var node = Settings.Nodes.FirstOrDefault(i => i.Net == net);

		if(node?.Process != null && !node.Process.HasExited)
		{
			node.Process.Kill();
		}

		if(node == null)
		{
			var info = IccpLcpServer.Relay(null, net, new InfoIcca(), null) as InfoIccr; /// get wayin info

			var wi = info.Wayins.FirstOrDefault(i => i.Software != null);
				
			if(wi != null)
			{
				Snq.Parse(wi.Software, out var s, out var n, out var q);
			
				if(s == Iccp.Scheme) /// deploy node software
				{
					var p = DeployProduct(Ura.Parse(wi.Software), flow);

					node = new DeployedNode {Net = net, Package = p.Resource.Address.ToString()};
					Settings.Nodes.Add(node);
				}
				else
					throw new NexusException("Software source is not supported");
			}
			else
				throw new NexusException("No node software");

//
//			if(wi.Command != null)
//			{
//				var ps = new Process();
//
//				ps.StartInfo.FileName = null;
//				ps.StartInfo.Arguments = wi.Command;
//				ps.StartInfo.UseShellExecute = true;
//		
//				ps.Start();
//			}
		}

		node.Process = Run(Ura.Parse(node.Package));

		return node;
	}

	public byte[] Do(Snq snq, Flow flow)
	{
		if(snq.Net == null || snq.Net == Iccn.Root)
		{
			return RdnDo(new Ura(snq), flow);
		}
		else
		{
			var c = IccpLcpServer.Locals.FirstOrDefault(i => i.Net == snq.Net);

			if(c == null)
			{
				RunNode(snq.Net, flow);
			
//				if(wi.Command != null)
//				{
//					var ps = new Process();
//
//					ps.StartInfo.FileName = null;
//					ps.StartInfo.Arguments = wi.Command;
//					ps.StartInfo.UseShellExecute = true;
//		
//					ps.Start();
//				}
				
			}
			
			while(c == null)
			{	
				if(!flow.Active)
					return null;

				c = IccpLcpServer.Locals.FirstOrDefault(i => i.Net == snq.Net);

				if(c == null)
					Thread.Sleep(10);
			}
			
			return c.Call<DoIccr>(null, snq.Net, new DoIcca {Query = snq.Query}, null).Response;
		}
	}

	LocalPackage DeployProduct(Ura ura, Flow flow)
	{
		var r = RdnNode.Peering.Call(new ResourceByAddressPpc(ura), flow)?.Resource;
		//var d = RdnNode.ResourceHub.Find(ura)?.Last;
				
		if(r.Data == null)
			throw new NexusException("No data");
	
		//Ura apr = null;
		Ura aprv = null;
	
		if(r.Data.Type.Content == ContentType.Package_Software_ProductManifest)
		{
			//var lrr = RdnNode.Download(r, flow);
	
			//lock(RdnNode.ResourceHub.Lock)
			{
				var m = new Reader(r.Data.Value).Read<ProductManifest>();
	
				aprv = m.Realizations.FirstOrDefault(i => i.Condition.Match(Platform.Current)).Latest;
			}
		}
		else if(r.Data.Type.Content == ContentType.Package_Software_VersionManifest)
		{
			aprv = ura;
		}
		else
			throw new NexusException("Incorrect resource type");
	
		return PackageHub.Deploy(aprv, flow);

	}

	byte[] RdnDo(Ura ura, Flow flow)
	{
		var p = DeployProduct(ura, flow);

		Run(p.Resource.Address);

		return null;
	}

	Process Run(Ura package)
	{
//		var vmpath = Directory.EnumerateFiles(PackageHub.AddressToDeployment(Settings.Packages, package), "*." + PackageManifest.Extension).First();
//	
//		var vm = PackageManifest.Load(vmpath);
//	
//		var exe = vm.MatchExecution(Platform.Current);

		

		var m =	PackageHub.Find(package).Manifest;

		SetupApplicationEnvironemnt(package);
	
		var ps = new Process();
		ps.StartInfo.UseShellExecute = true;
		ps.StartInfo.FileName = Path.Join(PackageHub.AddressToDeployment(Settings.Packages, package), m.Start[0].Path);
		ps.StartInfo.Arguments = m.Start[0].Arguments;
	
		ps.Start();

		return ps;
	}

	public void SetupApplicationEnvironemnt(Ura address)
	{
		Environment.SetEnvironmentVariable(Application.PackageAddressKey,	address.ToString());
		Environment.SetEnvironmentVariable(Application.ProfileKey,			Settings.Profile);

		Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, address);
	}

	public byte[] GetApplicationSession(string net, Flow flow)
	{
		var s = Settings.Sessions.FirstOrDefault(i => i.Net == net);

		if(s == null)
		{
			lock(Vault)
				s = new NexusSessionSettings
					{
						Net = net,
						Session = Vault.Authenticate(Settings.Name, net, "", null, null, flow).Session
					};

			Settings.Sessions = [..Settings.Sessions, s];
			Settings.Save();
		}

		return s.Session;
	}
}	
