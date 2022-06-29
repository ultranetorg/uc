using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class Seeder
	{
		public IPAddress	IP;
		public DateTime		Arrived;

		public Seeder(IPAddress ip, DateTime arrived)
		{
			IP = ip;
			Arrived = arrived;
		}
	}

 	public class Package
 	{
		//public Peer				SearchInitiator;
		public List<Seeder>	Seeders = new();
 	}

	public class Hub
	{
		Core										Core;
		public Dictionary<PackageAddress, Package>	Packages = new ();
		public const int							SeedersPerPackageMax = 1000; /// (100_000 packages * 20) + (100_000 * 100 versions * 4) + (100_000 * 100 * 1000 seders * 4) ~= 40 gb
		public const int							SeedersPerRequestMax = 256;

		public Hub(Core core)
		{
			Core = core;
		}

		Package GetPackage(PackageAddress package)
		{
 			if(!Packages.ContainsKey(package))
 				return Packages[package] = new();
 			else
 				return Packages[package];
		}

		public void Declare(IPAddress seeder, IEnumerable<PackageAddress> packages)
		{
			foreach(var i in packages)
			{
				var p = GetPackage(i);
	
				var s = p.Seeders.Find(i => i.IP.Equals(seeder));

				if(s == null)
				{
					p.Seeders.Add(new Seeder(seeder, DateTime.UtcNow));
				} 
				else
				{
					s.Arrived = DateTime.UtcNow;
				}
		
				if(p.Seeders.Count > SeedersPerPackageMax)
				{
					p.Seeders = p.Seeders.OrderByDescending(i => i.Arrived).ToList();
					p.Seeders.RemoveRange(SeedersPerPackageMax, p.Seeders.Count - SeedersPerPackageMax);
				}
			}
		}

 		public IPAddress[] Locate(LocatePackageRequest request)
 		{
 			if(Packages.ContainsKey(request.Package))
	 			return Packages[request.Package].Seeders.OrderByDescending(i => i.Arrived).Take(Math.Min(request.Count, SeedersPerRequestMax)).Select(i => i.IP).ToArray();
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
