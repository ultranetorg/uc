using System.Net;

namespace Uccs.Rdn
{
	public class PackageAddApc : RdnApc
	{
		public Ura						Resource { get; set; }
		public ResourceId				Id { get; set; }
		public byte[]					Complete { get; set; }
		public byte[]					Incremental { get; set; }
		public byte[]					Manifest { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var h = node.Zone.Cryptography.HashFile(Manifest);
			var a = AddressCreator.Create(node.Mcv, h);

			lock(node.PackageHub.Lock)
			{
				var p = node.PackageHub.Get(Resource);
				
				lock(node.ResourceHub.Lock)
				{
					p.Resource.Id = Id;
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

		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.PackageHub.Lock)
				return sun.PackageHub.AddRelease(Resource, Sources, DependenciesPath, Previous, AddressCreator, workflow);
		}
	}

	public class PackageDownloadApc : RdnApc
	{
		public Ura		Package { get; set; }

		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.PackageHub.Lock)
			{	
				sun.PackageHub.Download(Package, workflow);
				return null;
			}
		}
	}

	public class PackageActivityProgressApc : RdnApc
	{
		public Ura	Package { get; set; }
		
		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var p = sun.PackageHub.Find(Package);

			lock(sun.PackageHub.Lock)
				if(p?.Activity is PackageDownload dl)
					return new PackageDownloadProgress(dl);
				if(p?.Activity is Deployment dp)
					return new DeploymentProgress(dp);
				else
					return null;
		}
	}

	public class PackageInfoApc : RdnApc
	{
		public Ura	Package { get; set; }
		
		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.PackageHub.Lock)
			{
				var p = sun.PackageHub.Find(Package);

				if(p == null)
					return null;

				return new PackageInfo{ Ready			= sun.PackageHub.IsReady(Package),
										Availability	= p.Release.Availability,
										Manifest		= p.Manifest };
			}
		}
	}

	public class PackageInfo
	{
		public bool				Ready { get; set; }
		public Availability		Availability { get; set; }
		public PackageManifest	Manifest { get; set; }
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
	//	public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	//	{
	//		lock(sun.PackageHub.Lock)
	//		{
	//			var p = sun.PackageHub.AddressToDeployment(Package);
	//
	//			var h = Path.Join(p, ".hash");
	//
	//			return new Result {	Path = p,
	//								Hash = File.Exists(h) ? File.ReadAllText(h).FromHex() : null};
	//		}
	//	}
	//}
}