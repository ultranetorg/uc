using System;
using System.Collections.Generic;
using System.IO;

namespace Uccs.Net
{ 
	public class PackageAddCall : SunApiCall
	{
		public PackageAddress	Package { get; set; }
		public byte[]			Complete { get; set; }
		public byte[]			Incremental { get; set; }
		public byte[]			Manifest { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			var m = new Manifest();
			m.Read(new BinaryReader(new MemoryStream(Manifest)));
								
			var h = sun.Zone.Cryptography.HashFile(m.Bytes);
								
			lock(sun.ResourceHub.Lock)
			{
				var r = sun.ResourceHub.Add(Package, ResourceType.Package, h);
	
				r.AddFile(Net.Package.ManifestFile, Manifest);
	
				if(Complete != null)
					r.AddFile(Net.Package.CompleteFile, Complete);

				if(Incremental != null)
					r.AddFile(Net.Package.IncrementalFile, Incremental);
								
				r.Complete((Complete != null ? Availability.Complete : 0) | (Incremental != null ? Availability.Incremental : 0));
				sun.ResourceHub.SetLatest(Package, h);
			}

			return null;
		}
	}

	public class PackageBuildCall : SunApiCall
	{
		public PackageAddress		Package { get; set; }
		public Version				Version { get; set; }
		public IEnumerable<string>	Sources { get; set; }
		public string				DependsDirectory { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				sun.PackageHub.AddRelease(Package, Version, Sources, DependsDirectory, workflow);

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