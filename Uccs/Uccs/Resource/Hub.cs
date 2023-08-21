using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class Seed
	{
		public IPAddress			IP;
		public DateTime				Arrived;
		public Availability			Availability;

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

	public class Hub
	{
		Sun										Sun;
		public Dictionary<byte[], List<Seed>>	Releases = new (new BytesEqualityComparer());
		public const int						SeedersPerPackageMax = 1000; /// (1000000 authors * 5 products * 1 rlzs * 100 versions * 1000 peers)*4 ~= 2 TB
		public const int						SeedersPerRequestMax = 256;
		public object							Lock = new object();

		public Hub(Sun sun)
		{
			Sun = sun;
		}

		List<Seed> GetSeeds(byte[] release)
		{
 			if(!Releases.ContainsKey(release))
 				return Releases[release] = new();
 			else
 				return Releases[release];
		}

		public void Add(IPAddress ip, IEnumerable<DeclareReleaseItem> packages)
		{
			foreach(var i in packages)
			{
				var ss = GetSeeds(i.Hash);
	
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

				s.Availability = i.Availability;
		
				if(ss.Count > SeedersPerPackageMax)
				{
					ss = ss.OrderByDescending(i => i.Arrived).ToList();
					ss.RemoveRange(SeedersPerPackageMax, ss.Count - SeedersPerPackageMax);
				}
			}
		}

 		public IPAddress[] Locate(LocateReleaseRequest request)
 		{
 			if(Releases.ContainsKey(request.Hash))
	 			return Releases[request.Hash].OrderByDescending(i => i.Arrived).Take(Math.Min(request.Count, SeedersPerRequestMax)).Select(i => i.IP).ToArray();
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
