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
						rl.AddFile(Net.Package.ManifestFile, Manifest);
			
						if(Complete != null)
							rl.AddFile(Net.Package.CompleteFile, Complete);
		
						if(Incremental != null)
							rl.AddFile(Net.Package.IncrementalFile, Incremental);
										
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
				sun.PackageHub.Download(Package, workflow);

			return null;
		}
	}

	public class PackageDownloadProgressCall : SunApiCall
	{
		public PackageAddress	Package { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				return sun.PackageHub.GetDownloadProgress(Package);
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
					throw new RdcEntityException(RdcEntityError.NotFound);

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

	public class PackageInstallCall : SunApiCall
	{
		public PackageAddress	Package { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				sun.PackageHub.Install(Package, workflow);

			return null;
		}
	}
}