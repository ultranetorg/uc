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
		public PackageAddress			Address;
		public Package					Package;
		public bool						Downloaded;
		public List<PackageDownload>	Dependencies = new();
		public Task						Task;
		public SeedCollector			SeedCollector;

		public PackageDownload(PackageAddress address)
		{
			Address = address;
		}

		public bool									Succeeded => Downloaded && /*DependenciesRecursiveFound && */DependenciesRecursiveCount == DependenciesRecursiveSuccesses;
		public int									DependenciesRecursiveCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesRecursiveCount);
		//public bool									DependenciesRecursiveFound => Package.Manifest != null && Dependencies.All(i => i.DependenciesRecursiveFound);
		public int									DependenciesRecursiveSuccesses => Dependencies.Count(i => i.Succeeded) + Dependencies.Sum(i => i.DependenciesRecursiveSuccesses);
		public IEnumerable<PackageDownload>			DependenciesRecursive => Dependencies.Union(Dependencies.SelectMany(i => i.DependenciesRecursive)).DistinctBy(i => i.Package);

		public override string ToString()
		{
			return Package.ToString();
		}
	}
}
