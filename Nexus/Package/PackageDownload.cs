using Uccs.Rdn;

namespace Uccs.Nexus;

public class PackageDownloadProgress : ResourceActivityProgress
{
	public bool							Succeeded { get; set; }
	public int							DependenciesRecursiveCount { get; set; }
	public int							DependenciesRecursiveSuccesses { get; set; }
	public FileDownloadProgress[]		CurrentFiles { get; set; } = [];
	public PackageDownloadProgress[]	Dependencies { get; set; } = [];

	public PackageDownloadProgress()
	{
	}

	public PackageDownloadProgress(PackageDownload download)
	{
		Succeeded						= download.Succeeded;
		DependenciesRecursiveCount		= download.DependenciesRecursiveCount;
		DependenciesRecursiveSuccesses	= download.DependenciesRecursiveSuccesses;

		lock(download.Package.Hub.Node.ResourceHub.Lock)
		{
			CurrentFiles = download.Package.Release?.Files	.Where(i => i.Activity is FileDownload)
															.Select(i => new FileDownloadProgress(i.Activity as FileDownload))
															.ToArray();
		}
			
		Dependencies = download.Dependencies.Where(i => i.Package.Activity is PackageDownload)
											.Select(i => new PackageDownloadProgress(i.Package.Activity as PackageDownload))
											.ToArray();
	}

	public override string ToString()
	{
		return$"downloading: {{{(CurrentFiles != null ? string.Join(", ", CurrentFiles?.Select(i => $"{i.Path}={i.DownloadedLength}/{i.Length}")) : null)}}}, dps: {DependenciesRecursiveSuccesses}/{DependenciesRecursiveCount}";
	}
}

public class PackageDownload
{
	public LocalPackage						Package;
	public FileDownload						FileDownload;
	public bool								IsDownloaded;
	public List<PackageDownload>			Dependencies = new();
	public Task								Task;
	public SeedSeeker						Seeker;

	public bool								Succeeded => IsDownloaded && DependenciesRecursiveCount == DependenciesRecursiveSuccesses;
	public int								DependenciesRecursiveCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesRecursiveCount);
	public int								DependenciesRecursiveSuccesses => Dependencies.Count(i => i.Succeeded) + Dependencies.Sum(i => i.DependenciesRecursiveSuccesses);
	public IEnumerable<PackageDownload>		DependenciesRecursive => Dependencies.Concat(Dependencies.SelectMany(i => i.DependenciesRecursive)).DistinctBy(i => i.Package);

	public PackageDownload(PackageHub hub, LocalPackage package, Flow workflow)
	{
		Package = package;

		var node = hub.Node;

		lock(hub.Lock)
		{
			if(hub.IsAvailable(package.Resource.Address))
			{
				IsDownloaded = true;
				return;
			}
		}

		Package.Activity = this;

		Task = Task.Run(() =>	{
									try
									{
										Resource last = null;

										while(workflow.Active)
										{
											try
											{
												last = node.Peering.Call(() => new ResourceRequest(package.Resource.Address), workflow).Resource;
													
												if(last.Data?.Type != new DataType(DataType.File, ContentType.Rdn_VersionManifest))
												{
													Package.Activity = null;
													return;
												}

												break;
											}
											catch(EntityException)
											{
												Thread.Sleep(100);
											}
										}

										lock(node.ResourceHub.Lock)
										{
											node.ResourceHub.Add(last.Data.Parse<Urr>());
											package.Resource.AddData(last.Data);
										}

										IIntegrity itg = null;

										switch(last.Data.Parse<Urr>())
										{ 
											case Urrh u:
												itg = new DHIntegrity(u.Hash); 
												break;

											case Urrsd u:
												var d = node.Peering.Call(() => new DomainRequest(package.Resource.Address.Domain), workflow).Domain;
												var aa = node.Peering.Call(() => new AccountRequest(d.Owner), workflow).Account;
												itg = new SPDIntegrity(node.Net.Cryptography, u, aa.Address);
												break;
										};

										Seeker = new SeedSeeker(node, package.Release.Address, workflow);

										node.ResourceHub.GetFile(Package.Release, false, LocalPackage.ManifestFile, Path.Join(hub.AddressToReleases(last.Data.Parse<Urr>()), LocalPackage.ManifestFile), itg, Seeker, workflow);

										bool incrementable;

										lock(hub.Lock)
										{
											hub.DetermineDelta(package.Resource.Address, Package.Manifest, out incrementable, out List<Dependency> deps);
								
											foreach(var i in deps.Where(i => i.Need == DependencyNeed.Critical))
											{
												if(!hub.ExistsRecursively(i.Address))
												{
													var dd = hub.Download(i.Address, workflow);
													Dependencies.Add(dd);
												}
											}
										}

										lock(node.ResourceHub)
 											FileDownload = node.ResourceHub.DownloadFile(Package.Release, 
																						false,
																						incrementable ? LocalPackage.IncrementalFile : LocalPackage.CompleteFile, 
																						Path.Join(hub.AddressToReleases(last.Data.Parse<Urr>()), incrementable ? LocalPackage.IncrementalFile : LocalPackage.CompleteFile),
																						new DHIntegrity(incrementable ? Package.Manifest.IncrementalHash : Package.Manifest.CompleteHash),
																						Seeker,
																						workflow);


										Task.WaitAll(DependenciesRecursive.Select(i => i.Task).Append(FileDownload.Task).ToArray());

										Seeker.Stop();

										var a = Availability.None;

										lock(node.ResourceHub.Lock)
										{
											if(Package.Release.IsReady(LocalPackage.CompleteFile))
												a |= Availability.Complete;

											if(Package.Release.IsReady(LocalPackage.IncrementalFile))
												a |= Availability.Incremental;
										}

										lock(hub.Lock)
										{
											Package.Release.Complete(a);
											IsDownloaded = true;
										}
									}
									catch(Exception) when(workflow.Aborted)
									{
									}
									finally
									{
										lock(hub.Lock)
											Package.Activity = null;
									}
								},
								workflow.Cancellation);
	}

	public override string ToString()
	{
		return Package.ToString();
	}
}
