using System.Net;

namespace Uccs.Rdn
{
	public class PackageAddApc : RdnApc
	{
		public Ura						Resource { get; set; }
		public byte[]					Complete { get; set; }
		public byte[]					Incremental { get; set; }
		public byte[]					Manifest { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var h = node.Net.Cryptography.HashFile(Manifest);
			var a = AddressCreator.Create(node.Mcv, h);

			lock(node.PackageHub.Lock)
			{
				var p = node.PackageHub.Get(Resource);
				
				lock(node.ResourceHub.Lock)
				{
					p.Resource.AddData(new ResourceData(new DataType(DataType.File, ContentType.Rdn_PackageManifest), a));
					
					var r = node.ResourceHub.Find(a) ?? node.ResourceHub.Add(a);

					var path = node.PackageHub.AddressToReleases(a);

					r.AddCompleted(LocalPackage.ManifestFile, Path.Join(path, LocalPackage.ManifestFile), Manifest);
			
					if(Complete != null)
						r.AddCompleted(LocalPackage.CompleteFile, Path.Join(path, LocalPackage.CompleteFile), Complete);
		
					if(Incremental != null)
						r.AddCompleted(LocalPackage.IncrementalFile, Path.Join(path, LocalPackage.IncrementalFile), Incremental);
										
					r.Complete((Complete != null ? Availability.Complete : 0) | (Incremental != null ? Availability.Incremental : 0));
				}
			}

			return null;
		}
	}

	public class PackageBuildApc : RdnApc
	{
		public Ura						Resource { get; set; }
		public IEnumerable<string>		Sources { get; set; }
		public string					DependenciesPath { get; set; }
		public Ura						Previous { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(node.PackageHub.Lock)
			{	
				return new LocalReleaseApe(node.PackageHub.AddRelease(Resource, Sources, DependenciesPath, Previous, AddressCreator, workflow));
			}
		}
	}

	public class PackageDownloadApc : RdnApc
	{
		public Ura		Package { get; set; }

		public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(node.PackageHub.Lock)
			{	
				node.PackageHub.Download(Package, workflow);
				return null;
			}
		}
	}

	public class PackageActivityProgressApc : RdnApc
	{
		public Ura	Package { get; set; }
		
		public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(node.PackageHub.Lock)
			{
				var p = node.PackageHub.Find(Package);
	
				lock(node.PackageHub.Lock)
					if(p?.Activity is PackageDownload dl)
						return new PackageDownloadProgress(dl);
					if(p?.Activity is Deployment dp)
						return new DeploymentProgress(dp);
					else
						return null;
			}
		}
	}

	public class LocalPackageApc : RdnApc
	{
		public Ura	Address { get; set; }
		
		public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(node.PackageHub.Lock)
			{
				var p = node.PackageHub.Find(Address);

				if(p == null)
					return null;

				return new PackageInfo(p);
			}
		}
	}

	public class PackageInfo
	{
		public bool				Available { get; set; }
		public string			Path { get; set; }
		public VersionManifest	Manifest { get; set; }

		public PackageInfo()
		{
		}

		public PackageInfo(LocalPackage package)
		{
			Available = package.Hub.IsAvailable(package.Resource.Address);
			Manifest = package.Manifest;
		}
	}

	public class PackageDeployApc : RdnApc
	{
		public AprvAddress	Address { get; set; }
		public string		DeploymentPath { get; set; }
	
		public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
		{
			node.PackageHub.Deploy(Address, DeploymentPath ?? node.PackageHub.DeploymentPath, flow);
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
	//	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	//	{
	//		lock(node.PackageHub.Lock)
	//		{
	//			var p = node.PackageHub.AddressToDeployment(Package);
	//
	//			var h = Path.Join(p, ".hash");
	//
	//			return new Result {	Path = p,
	//								Hash = File.Exists(h) ? File.ReadAllText(h).FromHex() : null};
	//		}
	//	}
	//}
}