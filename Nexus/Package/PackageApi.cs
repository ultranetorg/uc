using System.Net;
using System.Text.Json.Serialization;
using Uccs.Rdn;

namespace Uccs.Nexus;

public abstract class PackageActivityProgress
{
}

public class PackageApe
{
	public bool					Available { get; set; }
	public LocalReleaseApe		Release { get; set; }
	public PackageManifest		Manifest { get; set; }

	public PackageApe()
	{
	}

	public PackageApe(Package package)
	{
		Manifest	= package.Manifest;
		Release		= new LocalReleaseApe(package.Release);
		Available	= package.Hub.IsAvailable(package.Id);
	}
}


//public class PackageAddApc : Apc, INexusApc 
//{
//	//public AutoId					Resource { get; set; }
//	public byte[]					Complete { get; set; }
//	public byte[]					Incremental { get; set; }
//	public byte[]					Manifest { get; set; }
//	public ReleaseAddressCreator	AddressCreator { get; set; }
//
//	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
//	{
//		var h = nexus.RdnNode.Net.Cryptography.HashFile(Manifest);
//		var a = AddressCreator.Create(null/*Vault*/, h);
//
//		lock(nexus.PackageHub.Lock)
//		{
//			//var p = nexus.PackageHub.Get(Resource);
//			
//			lock(nexus.RdnNode.ResourceHub.Lock)
//			{
//				//p.Resource.AddData(new ResourceData(new DataType(DataType.File, ContentType.Package_Software_VersionManifest), a));
//				
//				var r = nexus.RdnNode.ResourceHub.Find(a) ?? nexus.RdnNode.ResourceHub.Add(a);
//
//				var path = nexus.PackageHub.AddressToReleases(a);
//
//				r.AddCompleted(LocalPackage.ManifestFile, Path.Join(path, LocalPackage.ManifestFile), Manifest);
//		
//				if(Complete != null)
//					r.AddCompleted(LocalPackage.CompleteFile, Path.Join(path, LocalPackage.CompleteFile), Complete);
//	
//				if(Incremental != null)
//					r.AddCompleted(LocalPackage.IncrementalFile, Path.Join(path, LocalPackage.IncrementalFile), Incremental);
//									
//				r.Complete((Complete != null ? Availability.Complete : 0) | (Incremental != null ? Availability.Incremental : 0));
//
//				return new LocalReleaseApe(r);
//			}
//		}
//	}
//}

public class PackageBuildApc : Apc, INexusApc
{
	public IEnumerable<string>		Sources { get; set; }
	public string					Manifest { get; set; }
	public string					Instruction { get; set; }
	public AutoId					Previous { get; set; }
	public ReleaseAddressCreator	AddressCreator { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{	
			try
			{
				return new PackageApe(nexus.PackageHub.BuildRelease(Sources, 
																	Manifest == null ? new PackageManifest() : PackageManifest.FromXon(new Xon(Manifest)), 
																	Instruction == null ? null : PackageInstruction.FromXon(new Xon(Instruction)), 
																	Previous, 
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
	public AutoId		Id { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{	
			nexus.PackageHub.StartDownload(Id, workflow);
			return null;
		}
	}
}

public class PackageActivityProgressApc : Apc, INexusApc
{
	public AutoId	Id { get; set; }
	
	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{
			var p = nexus.PackageHub.Find(Id);

			if(p == null)
				throw new ResourceException(ResourceError.NotFound);

			lock(nexus.PackageHub.Lock)
				if(p?.Activity is PackageDownload dl)
					return new PackageDownloadProgress(dl);
				if(p?.Activity is Deployment dp)
					return new DeploymentProgress(dp);
				else
					return null;
		}
	}
}

public class LocalPackageApc : Apc, INexusApc
{
	public AutoId	Id { get; set; }
	
	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		lock(nexus.PackageHub.Lock)
		{
			var p = nexus.PackageHub.Find(Id);

			if(p == null)
				throw new ResourceException(ResourceError.NotFound);

			return new PackageApe(p);
		}
	}
}

public class PackageDeployApc : Apc, INexusApc
{
	public AutoId			Address { get; set; }
	public string		To { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(nexus.PackageHub == null)
			throw new ResourceException(ResourceError.NotHub);

		nexus.PackageHub.StartDeploy(Address, To ?? nexus.PackageHub.DeploymentPath, flow);
		return null;
	}
}

//public class DeploymentInfoApc : RdnApc
//{
//	public Ura	Package { get; set; }
//
//	public class Result
//	{
//		public byte[]		Hash { get; set; }
//		public string		Path { get; set; }
//	}
//
//	public override object Execute(RdnNode nexus, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
//	{
//		lock(nexus.PackageHub.Lock)
//		{
//			var p = nexus.PackageHub.AddressToDeployment(Package);
//
//			var h = Path.Join(p, ".hash");
//
//			return new Result {	Path = p,
//								Hash = File.Exists(h) ? File.ReadAllText(h).FromHex() : null};
//		}
//	}
//}