using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Uccs.Net
{
	public class PackageAddApc : RdsApc
	{
		public Ura						Resource { get; set; }
		public byte[]					Complete { get; set; }
		public byte[]					Incremental { get; set; }
		public byte[]					Manifest { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(Rds rds, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var h = rds.Zone.Cryptography.HashFile(Manifest);
			var a = AddressCreator.Create(rds, h);

			lock(rds.PackageHub.Lock)
			{
				var p = rds.PackageHub.Get(Resource);
				
				lock(rds.ResourceHub.Lock)
				{
					p.Resource.AddData(DataType.Package, a);
					
					var r = rds.ResourceHub.Find(a) ?? rds.ResourceHub.Add(a, DataType.Package);

					var path = rds.PackageHub.AddressToReleases(a);

					r.AddCompleted(LocalPackage.ManifestFile, Path.Join(path, LocalPackage.ManifestFile), Manifest);
			
					if(Complete != null)
						r.AddCompleted(LocalPackage.CompleteFile, Path.Join(path, LocalPackage.ManifestFile), Complete);
		
					if(Incremental != null)
						r.AddCompleted(LocalPackage.IncrementalFile, Path.Join(path, LocalPackage.ManifestFile), Incremental);
										
					r.Complete((Complete != null ? Availability.Complete : 0) | (Incremental != null ? Availability.Incremental : 0));
				}
			}

			return null;
		}
	}

	public class PackageBuildApc : RdsApc
	{
		public Ura						Resource { get; set; }
		public IEnumerable<string>		Sources { get; set; }
		public string					DependenciesPath { get; set; }
		public Ura						Previous { get; set; }
		public Ura[]					History { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(Rds sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.PackageHub.Lock)
				return sun.PackageHub.AddRelease(Resource, Sources, DependenciesPath, History, Previous, AddressCreator, workflow);
		}
	}

	public class PackageDownloadApc : RdsApc
	{
		public Ura		Package { get; set; }

		public override object Execute(Rds sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.PackageHub.Lock)
			{	
				sun.PackageHub.Download(Package, workflow);
				return null;
			}
		}
	}

	public class PackageInstallApc : RdsApc
	{
		public Ura	Package { get; set; }

		public override object Execute(Rds sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			sun.PackageHub.Install(Package, workflow);
			return null;
		}
	}

	public class PackageActivityProgressApc : RdsApc
	{
		public Ura	Package { get; set; }
		
		public override object Execute(Rds sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
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

	public class PackageInfoApc : RdsApc
	{
		public Ura	Package { get; set; }
		
		public override object Execute(Rds sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.PackageHub.Lock)
			{
				var p = sun.PackageHub.Find(Package);

				if(p == null)
					throw new ResourceException(ResourceError.NotFound);

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
		public Manifest			Manifest { get; set; }
	}
}