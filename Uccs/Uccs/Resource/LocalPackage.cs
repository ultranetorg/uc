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

		public PackageAddress	Address;
		public LocalRelease		Release;
		public LocalResource	Resource;
		PackageHub				Hub;
		Manifest				_Manifest;
		public object			Activity;

		public HistoryRelease	HistoryRelease => History.Releases.First(i => i.Hash.SequenceEqual(Address.Hash));
		public History			History => Hub.Sun.ResourceHub.Find(Address).LastAs<History>();

		public Manifest	Manifest
		{
			get
			{
				if(_Manifest == null)
				{
					_Manifest = new Manifest{};
					
					lock(Hub.Sun.ResourceHub.Lock)
					{
						_Manifest.Read(new BinaryReader(new MemoryStream(Release.ReadFile(ManifestFile))));
					}
				}

				return _Manifest;
			}
		}

		public LocalPackage(PackageHub hub, PackageAddress address, LocalResource resource, LocalRelease release)
		{
			if(resource == null || release == null)
				throw new ResourceException(ResourceError.BothResourceAndReleaseNotFound);

			Hub = hub;
			Address = address;
			Release = release;
			Resource = resource;
		}

		public LocalPackage(PackageHub hub, PackageAddress address, LocalResource resource, LocalRelease release, Manifest manifest)
		{
			if(resource == null || release == null)
				throw new ResourceException(ResourceError.BothResourceAndReleaseNotFound);

			Hub = hub;
			Address = address;
			Release = release;
			Resource = resource;
			_Manifest = manifest;
		}

		public override string ToString()
		{
			return Address.ToString();
		}

		public void AddRelease(byte[] hash)
		{
			var h = History == null ? new History {Releases = new()} : new History(History.Raw);

			h.Releases.Add(new HistoryRelease {Hash = hash});
			
			Resource.AddData(DataType.Package, h);
		}
	}
}
