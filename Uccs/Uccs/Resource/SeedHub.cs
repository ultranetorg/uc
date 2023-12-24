using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Nethereum.Contracts.QueryHandlers.MultiCall;

namespace Uccs.Net
{
	public class Seed
	{
		public IPAddress			IP;
		public DateTime				Arrived;
		public Availability			Availability;

		public Seed(IPAddress iP, DateTime arrived)
		{
			IP = iP;
			Arrived = arrived;
		}

		public override string ToString()
		{
			return IP.ToString();
		}
	}

	public class SeedHub
	{
		public const int						SeedsPerReleaseMax = 1000; /// (1000000 authors * 5 products * 1 rlzs * 100 versions * 1000 peers)*4 ~= 2 TB
		public const int						SeedsPerRequestMax = 256;
		Sun										Sun;
		public Dictionary<byte[], List<Seed>>	Releases = new (Bytes.EqualityComparer);
		public object							Lock = new object();

		public SeedHub(Sun sun)
		{
			Sun = sun;
		}

		public List<ReleaseDeclarationResult> ProcessIncoming(IPAddress ip, ResourceDeclaration[] resources)
		{
			var results = new List<ReleaseDeclarationResult>();

			foreach(var rs in resources)
			{
				Resource r;
				
				lock(Sun.Lock)
					r = Sun.Mcv.Authors.FindResource(rs.Resource, Sun.Mcv.LastConfirmedRound.Id);

				if(r == null || r.Data == null)
				{
					results.AddRange(rs.Releases.Select(i => new ReleaseDeclarationResult {Hash = i.Hash, Result = DeclarationResult.ResourceNotFound}));
					continue;
				}

				foreach(var rl in rs.Releases)
				{
					lock(Sun.Lock)
						if(!Sun.NextVoteMembers.OrderByNearest(rl.Hash).Take(ResourceHub.MembersPerDeclaration).Any(i => Sun.Settings.Generators.Contains(i.Account)))
						{
							results.Add(new ReleaseDeclarationResult {Hash = rl.Hash, Result = DeclarationResult.NotNearest});
							continue;
						}
					
					if(Releases.TryGetValue(rl.Hash, out var ss))
					{
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
						
						s.Availability = rl.Availability;
					}
					else
					{
						var d = new ResourceData(Sun.ResourceHub, r.Data);

						if((d.Type == DataType.File || d.Type == DataType.Directory) && d.Interpretation is byte[] h && !rl.Hash.SequenceEqual(h))
						{
							results.Add(new ReleaseDeclarationResult {Hash = rl.Hash, Result = DeclarationResult.ReleaseNotFound});
							continue;
						}

						if(d.Type == DataType.Package && !(d.Interpretation as History).Releases.Any(i => !rl.Hash.SequenceEqual(i.Hash)))
						{
							results.Add(new ReleaseDeclarationResult {Hash = rl.Hash, Result = DeclarationResult.ReleaseNotFound});
							continue;
						}

						Releases[rl.Hash] = ss = new () {new Seed(ip, DateTime.UtcNow) {Availability = rl.Availability}};
					}

					results.Add(new ReleaseDeclarationResult {Hash = rl.Hash, Result = DeclarationResult.Accepted});
			
					if(ss.Count > SeedsPerReleaseMax)
					{
						ss = ss.OrderByDescending(i => i.Arrived).ToList();
						ss.RemoveRange(SeedsPerReleaseMax, ss.Count - SeedsPerReleaseMax);
					}
				}
			}

			return results;
		}

 		public IPAddress[] Locate(LocateReleaseRequest request)
 		{
 			if(Releases.TryGetValue(request.Hash, out var v))
	 			return v.OrderByDescending(i => i.Arrived).Take(Math.Min(request.Count, SeedsPerRequestMax)).Select(i => i.IP).ToArray();
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
