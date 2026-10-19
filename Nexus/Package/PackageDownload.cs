using Uccs.Rdn;

namespace Uccs.Nexus;

public class PackageDownloadProgress : PackageActivityProgress
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
	public Package							Package;
	public FileDownload						FileDownload;
	public bool								IsDownloaded;
	public List<PackageDownload>			Dependencies = new();
	public Task								Task;
	public SeedSeeker						Seeker;

	public bool								Succeeded => IsDownloaded && DependenciesRecursiveCount == DependenciesRecursiveSuccesses;
	public int								DependenciesRecursiveCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesRecursiveCount);
	public int								DependenciesRecursiveSuccesses => Dependencies.Count(i => i.Succeeded) + Dependencies.Sum(i => i.DependenciesRecursiveSuccesses);
	public IEnumerable<PackageDownload>		DependenciesRecursive => Dependencies.Concat(Dependencies.SelectMany(i => i.DependenciesRecursive)).DistinctBy(i => i.Package);

	public PackageDownload(PackageHub hub, Package package, Flow flow)
	{
		Package = package;

		var node = hub.Node;

		lock(hub.Lock)
		{
			if(hub.IsAvailable(package.Address))
			{
				IsDownloaded = true;
				return;
			}
		}

		Package.Activity = this;

		Task = Task.Run(() =>	{
									try
									{
										//var r = hub.Get(Package.Id);
										
										//if(r != null && r.Meaning != Meaning.Package_Software_VersionManifest)
										//	throw new PackageException(PackageError.IncorrectContentType, $"{package.Id} is not {Meaning.Package_Software_VersionManifest}");

										Seeker = new SeedSeeker(node, Package.Urn, flow);

										var findex = node.ResourceHub.GetFile(Package.Release, false, Release.Index, null, new DHIntegrity((Package.Release.Address as Blake3Urn).Hash), Seeker, flow);

										var xindex = new Xon(findex.Read());

										node.ResourceHub.GetFile(	Package.Release, 
																	false, 
																	PackageManifest.Extension, 
																	Path.Join(hub.AddressToReleases(Package.Release.Address), PackageManifest.Extension), 
																	new DHIntegrity(xindex.Get<byte[]>(PackageManifest.Extension)), 
																	Seeker, 
																	flow);

										if(xindex.Has(PackageInstruction.Extension))
										{
											node.ResourceHub.GetFile(Package.Release, 
																	 false, 
																	 PackageInstruction.Extension, 
																	 Path.Join(hub.AddressToReleases(Package.Release.Address), PackageInstruction.Extension), 
																	 new DHIntegrity(xindex.Get<byte[]>(PackageInstruction.Extension)), 
																	 Seeker, 
																	 flow);
										}

										string file;

										lock(hub.Lock)
										{
											hub.DetermineDelta(Package.Manifest, out file, out var deps);
								
											foreach(var i in deps.Where(i => i.Need == DependencyNeed.Critical))
											{
												if(!hub.ExistsRecursively(i.Address))
												{
													var dd = hub.StartDownload(i.Address, flow);
													Dependencies.Add(dd);
												}
											}
										}

										lock(node.ResourceHub)
 											FileDownload = node.ResourceHub.DownloadFile(	Package.Release, 
																							false,
																							file, 
																							Path.Join(hub.AddressToReleases(Package.Release.Address), file),
																							new DHIntegrity(Package.Release.LoadIndex().One(file).Get<byte[]>()),
																							Seeker,
																							flow);


										Task.WaitAll(DependenciesRecursive.Select(i => i.Task).Append(FileDownload.Task).ToArray());

										var a = Availability.None;

										lock(node.ResourceHub.Lock)
										{
											if(Package.Release.IsReady(Package.CompleteFile))
												a |= Availability.Complete;

											if(Package.Release.Files.Any(i => Path.GetExtension(i.Path) == "." + Package.DeltaExtention && i.Status == ReleaseFileStatus.Completed))
												a |= Availability.Delta;
										}

										lock(hub.Lock)
										{
											Package.Release.Complete(a);
											IsDownloaded = true;
										}
									}
									catch(PackageException)
									{
									}
									catch(Exception) when(flow.Aborted)
									{
									}
									finally
									{
										Seeker?.Stop();

										lock(hub.Lock)
											Package.Activity = null;
									}
								},
								flow.Cancellation);
	}

	public override string ToString()
	{
		return Package.ToString();
	}
}
