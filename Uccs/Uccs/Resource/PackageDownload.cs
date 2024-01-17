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
			return$"Downloading={{{string.Join(", ", CurrentFiles.Select(i => $"{i.Path}={i.DownloadedLength}/{i.Length}"))}}}, Ds={DependenciesRecursiveSuccesses}/{DependenciesRecursiveCount}";
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

			lock(sun.PackageHub.Lock)
			{
				if(sun.PackageHub.IsReady(package))
				{
					Downloaded = true;
					return;
				}
			}

			Task = Task.Run(() =>	{
										try
										{
											History hst = null;
	
											while(workflow.Active)
											{
												try
												{
													hst = new History(ResourceData.SkipHeader(sun.Call(c => c.FindResource(package), workflow).Resource.Data));
													break;
												}
												catch(EntityException)
												{
													Thread.Sleep(100);
												}
											}
	
											SeedCollector = new SeedCollector(sun, package.Hash, workflow);
	
											lock(sun.PackageHub.Lock)
											{
												Package = sun.PackageHub.Get(package);
												Package.Resource.AddData(DataType.Package, hst);
											}
		 									
											sun.ResourceHub.GetFile(Package.Release, Package.ManifestFile, package.Hash, SeedCollector, workflow);
	
											bool incrementable;
	
											lock(sun.PackageHub.Lock)
											{
												sun.PackageHub.DetermineDelta(package, Package.Manifest, out incrementable, out List<Dependency> deps);
									
												foreach(var i in deps)
												{
													if(!sun.PackageHub.ExistsRecursively(i.Package))
													{
														var dd = sun.PackageHub.Download(i.Package, workflow);
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
