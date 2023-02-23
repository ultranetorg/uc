using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class Seed
	{
		public IPAddress		IP;
		public DateTime			Arrived;
		public Distributive		Distributives;

		public Seed(IPAddress ip, DateTime arrived)
		{
			IP = ip;
			Arrived = arrived;
		}

		public override string ToString()
		{
			return IP.ToString();
		}
	}

	public class Seedbase
	{
		Core											Core;
		public Dictionary<ReleaseAddress, List<Seed>>	Releases = new ();
		public const int								SeedersPerPackageMax = 1000; /// (1000000 authors * 5 products * 1 rlzs * 100 versions * 1000 peers)*4 ~= 2 TB
		public const int								SeedersPerRequestMax = 256;

		public Seedbase(Core core)
		{
			Core = core;
		}

		List<Seed> GetSeeders(ReleaseAddress release)
		{
 			if(!Releases.ContainsKey(release))
 				return Releases[release] = new();
 			else
 				return Releases[release];
		}

		public void Add(IPAddress ip, Dictionary<ReleaseAddress, Distributive> packages)
		{
			foreach(var i in packages)
			{
				var ss = GetSeeders(i.Key);
	
				var s = ss.Find(i => i.IP.Equals(ip));

				if(s == null)
				{
					s = new Seed(ip, DateTime.UtcNow);
					ss.Add(s);
				} 
				else
				{
					s.Arrived = DateTime.UtcNow;
				}

				s.Distributives = i.Value;
		
				if(ss.Count > SeedersPerPackageMax)
				{
					ss = ss.OrderByDescending(i => i.Arrived).ToList();
					ss.RemoveRange(SeedersPerPackageMax, ss.Count - SeedersPerPackageMax);
				}
			}
		}

 		public IPAddress[] Locate(LocateReleaseRequest request)
 		{
 			if(Releases.ContainsKey(request.Release))
	 			return Releases[request.Release].OrderByDescending(i => i.Arrived).Take(Math.Min(request.Count, SeedersPerRequestMax)).Select(i => i.IP).ToArray();
 			else
 				return new IPAddress[0]; /// TODO: ask other hubs
 		}

		void Searching()
		{

		}

// 		public IPAddress[] AddSeeders(PackageAddress package, IEnumerable<IPAddress> ips)
// 		{
// 			Package pp = null;
// 
// 			if(!Packages.ContainsKey(package))
// 				Packages[package] = pp = new();
// 			else
// 				pp = Packages[package];
// 
// 			var peers = ips.Where(ip => !pp.Seeders.Any(j => j.IP.Equals(ip))).Select(i => new Seeder(i, DateTime.UtcNow));
// 
// 			pp.Seeders.AddRange(peers);
// 
// 			if(pp.Seeders.Count > PeersPerPackageMax)
// 			{
// 				var last = pp.Seeders.OrderByDescending(i => i.Arrived).Skip(PeersPerPackageMax).First().Arrived;
// 				pp.Seeders.RemoveAll(i => i.Arrived <= last);
// 			}
// 
// 			return peers.Select(i => i.IP).ToArray();
// 		}
	}
}
