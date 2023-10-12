using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class PackageDownloadProgress
	{
		public bool									Succeeded { get; set; }
		public int									DependenciesRecursiveCount { get; set; }
		public int									DependenciesRecursiveSuccesses { get; set; }
		public IEnumerable<FileDownloadProgress>	CurrentFiles { get; set; } = new FileDownloadProgress[]{};
		public IEnumerable<PackageDownloadProgress>	Dependencies { get; set; } = new PackageDownloadProgress[]{};

		public override string ToString()
		{
			return$"Fip={string.Join(", ", CurrentFiles.Select(i => $"{i.Path}={i.DownloadedLength}/{i.Length}"))}, Deps={DependenciesRecursiveSuccesses}/{DependenciesRecursiveCount}";
		}
	}

	public class PackageDownload
	{
		public PackageAddress					Address;
		public Package							Package;
		public FileDownload						FileDownload;
		public bool								Downloaded;
		public List<PackageDownload>			Dependencies = new();
		public Task								Task;
		public SeedCollector					SeedCollector;

		public bool								Succeeded => Downloaded && DependenciesRecursiveCount == DependenciesRecursiveSuccesses;
		public int								DependenciesRecursiveCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesRecursiveCount);
		public int								DependenciesRecursiveSuccesses => Dependencies.Count(i => i.Succeeded) + Dependencies.Sum(i => i.DependenciesRecursiveSuccesses);
		public IEnumerable<PackageDownload>		DependenciesRecursive => Dependencies.Union(Dependencies.SelectMany(i => i.DependenciesRecursive)).DistinctBy(i => i.Package);

		public PackageDownload(Sun sun, PackageAddress package, Workflow workflow)
		{
			Address = package;

			Task = Task.Run(() =>	{
										try
										{
											byte[] h = null;
											IEnumerable<PackageAddress> hst = null;
	
											while(workflow.Active)
											{
												try
												{
													hst = sun.Call(c => c.QueryResource(package.APR + "/"), workflow).Resources.Select(i => new PackageAddress(i)).OrderBy(i => i.Version);
													h = sun.Call(c => c.FindResource(package), workflow).Resource.Data;
													break;
												}
												catch(RdcEntityException)
												{
													Thread.Sleep(100);
												}
											}
	
											SeedCollector = new SeedCollector(sun, h, workflow);
	
											lock(sun.PackageHub.Lock)
												lock(sun.ResourceHub.Lock)
												{
													Package = sun.PackageHub.Find(package);
												
													if(Package != null)
													{
														if(Package.Release.Hash.SequenceEqual(h))
															goto done;
														else
															Package.Release = sun.ResourceHub.Add(package, ResourceType.Package, h); /// update to the latest
													} 
													else
													{	
														Package = new Package(sun.PackageHub, package, sun.ResourceHub.Add(package, ResourceType.Package, h));
														sun.PackageHub.Packages.Add(Package);
													}
												}
		 									
											sun.ResourceHub.GetFile(Package.Release, Package.ManifestFile, h, SeedCollector, workflow);
	
											bool incrementable;
	
											lock(sun.PackageHub.Lock)
											{
												sun.PackageHub.DetermineDelta(hst, Package.Manifest, h, out incrementable, out List<Dependency> deps);
									
												foreach(var i in deps)
												{
													if(!sun.PackageHub.ExistsRecursively(i.Release))
													{
														var dd = sun.PackageHub.Download(i.Release, workflow);
														Dependencies.Add(dd);
													}
												}
											}
	
	 										FileDownload = sun.ResourceHub.DownloadFile(Package.Release, 
																						incrementable ? Package.IncrementalFile : Package.CompleteFile, 
																						incrementable ? Package.Manifest.IncrementalHash : Package.Manifest.CompleteHash, 
																						SeedCollector,
																						workflow);
	
	
											Task.WaitAll(DependenciesRecursive.Select(i => i.Task).Append(FileDownload.Task).ToArray());
	
										done:
											SeedCollector.Stop();
	
											lock(sun.PackageHub.Lock)
											{
												var a = Availability.None;

												if(Package.Release.IsReady(Package.CompleteFile))
													a |= Availability.Complete;

												if(Package.Release.IsReady(Package.IncrementalFile))
													a |= Availability.Incremental;

												Package.Release.Complete(a);

												Downloaded = true;
											}
										}
										catch(Exception) when(workflow.Aborted)
										{
										}
										finally
										{
											lock(sun.PackageHub.Lock)
												sun.PackageHub.Downloads.Remove(this);
										}
									},
									workflow.Cancellation);
		}

		public override string ToString()
		{
			return Package.ToString();
		}
	}
}
