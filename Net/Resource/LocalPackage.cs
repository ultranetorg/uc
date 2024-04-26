using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class LocalPackage
	{
		public const string		IncrementalFile = "i";
		public const string		CompleteFile = "c";
		public const string		ManifestFile = "m";
		public const string		Removals = ".removals";
		public const string		Renamings = ".renamings"; /// TODO

		public LocalResource	Resource;
		public LocalRelease		Release => Resource.Last?.Interpretation is Urr a ? Hub.Sun.ResourceHub.Find(a) : null;
		public PackageHub		Hub;
		Manifest				_Manifest;
		public object			Activity;

		//public HistoryRelease	HistoryRelease => History.Releases.First(i => i.Hash.SequenceEqual(Address.Hash));
		//public History			History => Hub.Sun.ResourceHub.Find(Address).LastAs<History>();

		public Manifest	Manifest
		{
			get
			{
				if(_Manifest == null)
				{
					if(Release.IsReady(ManifestFile))
					{
						_Manifest = new Manifest{};
						
						lock(Hub.Sun.ResourceHub.Lock)
						{
							_Manifest.Read(new BinaryReader(new MemoryStream(Release.ReadFile(ManifestFile))));
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

		public LocalPackage(PackageHub hub, LocalResource resource, Manifest manifest)
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
