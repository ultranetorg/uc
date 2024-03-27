using System.Collections.Generic;
using System.Net;

namespace Uccs.Net
{
	public class PackageAddCall : SunApiCall
	{
		public ResourceAddress			Resource { get; set; }
		public byte[]					Complete { get; set; }
		public byte[]					Incremental { get; set; }
		public byte[]					Manifest { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			var h = sun.Zone.Cryptography.HashFile(Manifest);
			var a = AddressCreator.Create(sun, h);

			lock(sun.PackageHub.Lock)
			{
				var p = sun.PackageHub.Get(Resource);
				
				lock(sun.ResourceHub.Lock)
				{
					p.Resource.AddData(DataType.Package, a);
					
					var r = sun.ResourceHub.Find(a) ?? sun.ResourceHub.Add(a, DataType.Package);

					r.AddCompleted(LocalPackage.ManifestFile, Manifest);
			
					if(Complete != null)
						r.AddCompleted(LocalPackage.CompleteFile, Complete);
		
					if(Incremental != null)
						r.AddCompleted(LocalPackage.IncrementalFile, Incremental);
										
					r.Complete((Complete != null ? Availability.Complete : 0) | (Incremental != null ? Availability.Incremental : 0));
				}
			}

			return null;
		}
	}

	public class PackageBuildCall : SunApiCall
	{
		public ResourceAddress			Resource { get; set; }
		public IEnumerable<string>		Sources { get; set; }
		public string					DependenciesPath { get; set; }
		public ResourceAddress			Previous { get; set; }
		public ResourceAddress[]		History { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				sun.PackageHub.AddRelease(Resource, Sources, DependenciesPath, History, Previous, AddressCreator, workflow);

			return null;
		}
	}

	public class PackageDownloadCall : SunApiCall
	{
		public ResourceAddress		Package { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
			{	
				sun.PackageHub.Download(Package, workflow);
				return null;
			}
		}
	}

	public class PackageInstallCall : SunApiCall
	{
		public ResourceAddress	Package { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			sun.PackageHub.Install(Package, workflow);
			return null;
		}
	}

	public class PackageActivityProgressCall : SunApiCall
	{
		public ResourceAddress	Package { get; set; }
		
		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
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

	public class PackageInfoCall : SunApiCall
	{
		public ResourceAddress	Package { get; set; }
		
		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
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