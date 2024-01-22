using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Uccs.Net
{ 
	public class PackageAddCall : SunApiCall
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Complete { get; set; }
		public byte[]			Incremental { get; set; }
		public byte[]			Manifest { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			var h = sun.Zone.Cryptography.HashFile(Manifest);
			
			lock(sun.PackageHub.Lock)
			{
				var p = sun.PackageHub.Get(new (Resource, h));
				p.AddRelease(h);
				
				lock(sun.ResourceHub.Lock)
				{
					var rl = sun.ResourceHub.Find(h);
	
					if(rl == null)
					{
						rl.AddCompleted(Net.LocalPackage.ManifestFile, Manifest);
			
						if(Complete != null)
							rl.AddCompleted(Net.LocalPackage.CompleteFile, Complete);
		
						if(Incremental != null)
							rl.AddCompleted(Net.LocalPackage.IncrementalFile, Incremental);
										
						rl.Complete((Complete != null ? Availability.Complete : 0) | (Incremental != null ? Availability.Incremental : 0));
					}
				}
			}

			return null;
		}
	}

	public class PackageBuildCall : SunApiCall
	{
		public ResourceAddress		Resource { get; set; }
		public IEnumerable<string>	Sources { get; set; }
		public string				DependenciesPath { get; set; }
		public byte[]				Previous { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				sun.PackageHub.AddRelease(Resource, Sources, DependenciesPath, Previous, workflow);

			return null;
		}
	}

	public class PackageDownloadCall : SunApiCall
	{
		public PackageAddress		Package { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
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
		public PackageAddress	Package { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			sun.PackageHub.Install(Package, workflow);
			return null;
		}
	}

	public class PackageActivityProgressCall : SunApiCall
	{
		public PackageAddress	Package { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
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
		public PackageAddress	Package { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
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