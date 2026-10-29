using System.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

public abstract class PackageActivityProgress
{
}

public class PackageApe
{
	public Ura					Address { get; set; }
	public LocalReleaseApe		Release { get; set; }
	public PackageManifest		Manifest { get; set; }
	public bool					Available { get; set; }

	public PackageApe()
	{
	}

	public PackageApe(Package package)
	{
		Address		= package.Address;
		Manifest	= package.Manifest;
		Release		= new LocalReleaseApe(package.Release);
		Available	= package.Hub.IsReady(package.Address);
	}
}

public class PackageCreateApc : Fpc<PackageApe>, INexusApc
{
	public Ura						Address { get; set; }
	public IEnumerable<string>		Sources { get; set; }
	public string					Manifest { get; set; }
	public string					Instruction { get; set; }
	public ReleaseAddressCreator	AddressCreator { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{	
			try
			{
				return new PackageApe(nexus.PackageHub.Create(	Address,
																Sources, 
																Manifest == null ? new PackageManifest() : PackageManifest.FromXon(new Xon(Manifest)), 
																Instruction == null ? null : PackageInstruction.FromXon(new Xon(Instruction)), 
																AddressCreator, 
																flow));
			}
			catch(IOException ex)
			{
				throw new PackageException(PackageError.IO, ex.Message);
			}
		}
	}
}

public class StartPackageDownloadApc : Apc, INexusApc
{
	public Ura		Address { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{	
			nexus.PackageHub.StartDownload(Address, true, workflow);
			return null;
		}
	}
}

public class PackageActivityProgressApc : Fpc<PackageActivityProgress>, INexusApc
{
	public Ura	Address { get; set; }
	
	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{
			var p = nexus.PackageHub.Find(Address);

			if(p == null)
				throw new ResourceException(ResourceError.NotFound);

			lock(nexus.PackageHub.Lock)
				if(p.Activity is PackageDownload dl)
					return new PackageDownloadProgress(dl);
				if(p.Activity is Deployment dp)
					return new DeploymentProgress(dp);
				else
					return null;
		}
	}
}

public class LocalPackageApc : Fpc<PackageApe>, INexusApc
{
	public Ura	Address { get; set; }
	
	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{
			var p = nexus.PackageHub.Find(Address);

			if(p == null)
				throw new ResourceException(ResourceError.NotFound);

			return new PackageApe(p);
		}
	}
}

public class LocalPackageSearchApc : Fpc<PackageApe[]>, INexusApc
{
	public string	Query { get; set; }
	
	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{
			return nexus.PackageHub.Packages.Where(i => i.Address.ToString().Contains(Query, StringComparison.InvariantCultureIgnoreCase)).Select(i => new PackageApe(i)).ToArray();
		}
	}
}

public class PackageDeployApc : Apc, INexusApc
{
	public Ura			Address { get; set; }
	public string		To { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		nexus.PackageHub.StartDeploy(Address, To, flow);
		return null;
	}
}
