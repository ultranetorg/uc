using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class PackageDownload
	{
		public PackageAddress					Address;
		public Package							Package;
		public FileDownload						FileDownload;
		public bool								Downloaded;
		public List<PackageDownload>			Dependencies = new();
		public Task								Task;
		public SeedCollector					SeedCollector;

		public bool								Succeeded => Downloaded && /*DependenciesRecursiveFound && */DependenciesRecursiveCount == DependenciesRecursiveSuccesses;
		public int								DependenciesRecursiveCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesRecursiveCount);
		//public bool							DependenciesRecursiveFound => Package.Manifest != null && Dependencies.All(i => i.DependenciesRecursiveFound);
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
	
											lock(sun.Packages.Lock)
												lock(sun.Resources.Lock)
												{
													Package = sun.Packages.Find(package);
												
													if(Package != null)
													{
														if(Package.Release.Hash.SequenceEqual(h))
															goto done;
														else
															Package.Release = sun.Resources.Add(package, h); /// update to the latest
													} 
													else
													{	
														Package = new Package(sun.Packages, package, sun.Resources.Add(package, h));
														sun.Packages.Packages.Add(Package);
													}
												}
		 									
											sun.Resources.GetFile(Package.Release, Package.ManifestFile, h, SeedCollector, workflow);
	
											bool incrementable;
	
											lock(sun.Packages.Lock)
											{
												sun.Packages.DetermineDelta(hst, Package.Manifest, h, out incrementable, out List<Dependency> deps);
									
												foreach(var i in deps)
												{
													if(!sun.Packages.ExistsRecursively(i.Release))
													{
														var dd = sun.Packages.Download(i.Release, workflow);
														Dependencies.Add(dd);
													}
												}
											}
	
	 										FileDownload = sun.Resources.DownloadFile(	Package.Release, 
																						incrementable ? Package.IncrementalFile : Package.CompleteFile, 
																						incrementable ? Package.Manifest.IncrementalHash : Package.Manifest.CompleteHash, 
																						SeedCollector,
																						workflow);
	
	
											Task.WaitAll(DependenciesRecursive.Select(i => i.Task).Append(FileDownload.Task).ToArray());
	
										done:
											SeedCollector.Stop();
	
											lock(sun.Packages.Lock)
											{
												var a = Availability.Null;;

												if(sun.Resources.Exists(Package.Release.Address, Package.Release.Hash, Package.CompleteFile))
													a |= Availability.CompleteFull;

												if(sun.Resources.Exists(Package.Release.Address, Package.Release.Hash, Package.IncrementalFile))
													a |= Availability.IncrementalFull;

												Package.Release.Complete(a);

												Downloaded = true;
											}
										}
										catch(Exception) when(workflow.Aborted)
										{
										}
										finally
										{
											lock(sun.Packages.Lock)
												sun.Packages.Downloads.Remove(this);
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
