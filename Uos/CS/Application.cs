using System.Reflection;

namespace Uccs.Uos;

public class Application
{
	public const string			ApiAddressEnvKey	= "UosApiAddress";
	public const string			ApiKeyEnvKey	 	= "UosApiKey";
	public const string			PackageAddressKey	= "PackageKey";
	public const string			PackagesPathKey		= "PackagesPathKey";

	public NexusClient			Nexus;
	public ApvAddress			Address => ApvAddress.Parse(Environment.GetEnvironmentVariable(PackageAddressKey));
	public string				PackagesPath => Environment.GetEnvironmentVariable(PackagesPathKey);

	public Application()
	{
		Nexus = new NexusClient();

		AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
	}

	Assembly AssemblyResolve(object sender, ResolveEventArgs args)
	{
		if(new AssemblyName(args.Name).Name.EndsWith(".resources"))
		{
			return null;
		}

		return Assembly.LoadFile(Path.Join(PackageHub.AddressToDeployment(PackagesPath, Address), new AssemblyName(args.Name).Name + ".dll"));

//  			var rp = Nexus.PackageHub.DeploymentToAddress(args.RequestingAssembly.Location);
//  
//  			var r = Nexus.PackageHub.Find(rp);
//  
//  			foreach(var i in r.Manifest.CriticalDependencies)
//  			{
//  				var dp = Path.Join(Nexus.PackageHub.AddressToDeployment(i.Package), new AssemblyName(args.Name).Name + ".dll");
//  
//  				if(File.Exists(dp))
//  				{
//  					return Assembly.LoadFile(dp);
//  				}
//  			}

//		return null;
	}
}
