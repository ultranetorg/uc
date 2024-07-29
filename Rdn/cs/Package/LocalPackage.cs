namespace Uccs.Rdn
{
	public class LocalPackage
	{
		public const string		IncrementalFile = "i";
		public const string		CompleteFile = "c";
		public const string		ManifestFile = "m";
		public const string		Removals = ".removals";
		public const string		Renamings = ".renamings"; /// TODO

		public LocalResource	Resource;
		public LocalRelease		Release => Resource.Last != null && Resource.Last.Type.Content == ContentType.Rdn_PackageManifest ? Hub.Node.ResourceHub.Find(Resource.Last.Parse<Urr>()) : null;
		public PackageHub		Hub;
		public object			Activity;
		PackageManifest			_Manifest;

		//public HistoryRelease	HistoryRelease => History.Releases.First(i => i.Hash.SequenceEqual(Address.Hash));
		//public History			History => Hub.Sun.ResourceHub.Find(Address).LastAs<History>();

		public PackageManifest	Manifest
		{
			get
			{
				if(_Manifest == null)
				{
					if(Release.IsReady(ManifestFile))
					{
						_Manifest = new PackageManifest{};
						
						lock(Hub.Node.ResourceHub.Lock)
						{
							_Manifest.Read(new BinaryReader(new MemoryStream(Release.Find(ManifestFile).Read())));
						}
					}
				}

				return _Manifest;
			}
		}

		public LocalPackage(PackageHub hub, LocalResource resource)
		{
			if(resource == null)
				throw new ResourceException(ResourceError.BothResourceAndReleaseNotFound);

			Hub = hub;
			Resource = resource;
		}

		public LocalPackage(PackageHub hub, LocalResource resource, PackageManifest manifest)
		{
			if(resource == null)
				throw new ResourceException(ResourceError.BothResourceAndReleaseNotFound);

			Hub = hub;
			Resource = resource;
			_Manifest = manifest;
		}

		public override string ToString()
		{
			return Resource.Address.ToString();
		}
	}
}
