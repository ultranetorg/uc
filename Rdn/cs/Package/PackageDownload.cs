namespace Uccs.Rdn
{
	public class PackageDownloadProgress : ResourceActivityProgress
	{
		public bool							Succeeded { get; set; }
		public int							DependenciesRecursiveCount { get; set; }
		public int							DependenciesRecursiveSuccesses { get; set; }
		public FileDownloadProgress[]		CurrentFiles { get; set; } = [];
		public PackageDownloadProgress[]	Dependencies { get; set; } = [];

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
		public bool								Downloaded;
		public List<PackageDownload>			Dependencies = new();
		public Task								Task;
		public SeedFinder						SeedCollector;

		public bool								Succeeded => Downloaded && DependenciesRecursiveCount == DependenciesRecursiveSuccesses;
		public int								DependenciesRecursiveCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesRecursiveCount);
		public int								DependenciesRecursiveSuccesses => Dependencies.Count(i => i.Succeeded) + Dependencies.Sum(i => i.DependenciesRecursiveSuccesses);
		public IEnumerable<PackageDownload>		DependenciesRecursive => Dependencies.Concat(Dependencies.SelectMany(i => i.DependenciesRecursive)).DistinctBy(i => i.Package);

		public PackageDownload(RdnNode node, LocalPackage package, Flow workflow)
		{
			Package = package;

			lock(node.PackageHub.Lock)
			{
				if(node.PackageHub.IsReady(package.Resource.Address))
				{
					Downloaded = true;
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
													last = node.Call(() => new ResourceRequest(package.Resource.Address), workflow).Resource;
														
													if(last.Data.Type != DataType.Package)
													{
														lock(node.PackageHub.Lock)
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
												node.ResourceHub.Add(last.Data.Interpretation as Urr, DataType.Package);
												package.Resource.AddData(last.Data);
											}

											IIntegrity itg = null;

											switch(last.Data.Interpretation)
											{ 
												case Urrh a :
													itg = new DHIntegrity(a.Hash); 
													break;

												case Urrsd a :
													var d = node.Call(() => new DomainRequest(package.Resource.Address.Domain), workflow).Domain;
													var aa = node.Call(() => new AccountRequest(d.Owner), workflow).Account;
													itg = new SPDIntegrity(node.Zone.Cryptography, a, aa.Address);
													break;
											};
	
											SeedCollector = new SeedFinder(node, package.Release.Address, workflow);
	
											node.ResourceHub.GetFile(Package.Release, LocalPackage.ManifestFile, Path.Join(node.PackageHub.AddressToReleases(last.Data.Interpretation as Urr), LocalPackage.ManifestFile), itg, SeedCollector, workflow);
	
											bool incrementable;
	
											lock(node.PackageHub.Lock)
											{
												node.PackageHub.DetermineDelta(package.Resource.Address, Package.Manifest, out incrementable, out List<Dependency> deps);
									
												foreach(var i in deps)
												{
													if(!node.PackageHub.ExistsRecursively(i.Package))
													{
														var dd = node.PackageHub.Download(i.Package, workflow);
														Dependencies.Add(dd);
													}
												}
											}
	
											lock(node.ResourceHub)
	 											FileDownload = node.ResourceHub.DownloadFile(Package.Release, 
																							incrementable ? LocalPackage.IncrementalFile : LocalPackage.CompleteFile, 
																							Path.Join(node.PackageHub.AddressToReleases(last.Data.Interpretation as Urr), incrementable ? LocalPackage.IncrementalFile : LocalPackage.CompleteFile),
																							new DHIntegrity(incrementable ? Package.Manifest.IncrementalHash : Package.Manifest.CompleteHash),
																							SeedCollector,
																							workflow);
	
	
											Task.WaitAll(DependenciesRecursive.Select(i => i.Task).Append(FileDownload.Task).ToArray());
	
											SeedCollector.Stop();
	
											lock(node.PackageHub.Lock)
											{
												var a = Availability.None;

												if(Package.Release.IsReady(LocalPackage.CompleteFile))
													a |= Availability.Complete;

												if(Package.Release.IsReady(LocalPackage.IncrementalFile))
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
											lock(node.PackageHub.Lock)
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
}
