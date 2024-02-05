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
		public const int								SeedsPerReleaseMax = 1000; /// (1000000 authors * 5 products * 1 rlzs * 100 versions * 1000 peers)*4 ~= 2 TB
		public const int								SeedsPerRequestMax = 256;
		Sun												Sun;
		public Dictionary<ReleaseAddress, List<Seed>>	Releases = new ();
		public object									Lock = new object();

		public SeedHub(Sun sun)
		{
			Sun = sun;
		}

		public List<ReleaseDeclarationResult> ProcessIncoming(IPAddress ip, ResourceDeclaration[] resources)
		{
			var results = new List<ReleaseDeclarationResult>();

			foreach(var rsd in resources)
			{
				Resource e;
				
				lock(Sun.Lock)
					e = Sun.Mcv.Authors.FindResource(rsd.Resource, Sun.Mcv.LastConfirmedRound.Id);

				if(e == null || e.Data == null)
				{
					results.AddRange(rsd.Releases.Select(i => new ReleaseDeclarationResult {Address = i.Address, Result = DeclarationResult.ResourceNotFound}));
					continue;
				}

				foreach(var d in rsd.Releases)
				{
					lock(Sun.Lock)
						if(!Sun.NextVoteMembers.OrderByNearest(d.Address.Hash).Take(ResourceHub.MembersPerDeclaration).Any(i => Sun.Settings.Generators.Contains(i.Account)))
						{
							results.Add(new ReleaseDeclarationResult {Address = d.Address, Result = DeclarationResult.NotNearest});
							continue;
						}
					
					if(Releases.TryGetValue(d.Address, out var ss))
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
						
						s.Availability = d.Availability;
					}
					else
					{
						if(e.Data.Interpretation is ReleaseAddress)
						{
							if(d.Address is HashAddress ha)
							{
								/// check existance

								if((e.Data.Interpretation as HashAddress) != ha)
								{
									results.Add(new ReleaseDeclarationResult {Address = d.Address, Result = DeclarationResult.ReleaseNotFound});
									continue;
								}
							}

							if(d.Address is ProvingAddress pa)
							{
								var ea = Sun.Mcv.Authors.Find(rsd.Resource.Author, Sun.Mcv.LastConfirmedRound.Id);

								if(!pa.Valid(Sun.Zone.Cryptography, rsd.Resource, ea.Owner))
								{
									results.Add(new ReleaseDeclarationResult {Address = d.Address, Result = DeclarationResult.ReleaseNotFound});
									continue;
								}
							}
						
							Releases[d.Address] = ss = new () {new Seed(ip, DateTime.UtcNow) {Availability = d.Availability}};
						}
						else
						{
							results.Add(new ReleaseDeclarationResult {Address = d.Address, Result = DeclarationResult.NotRelease});
							continue;
						}
					}

					results.Add(new ReleaseDeclarationResult {Address = d.Address, Result = DeclarationResult.Accepted});
			
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
 			if(Releases.TryGetValue(request.Address, out var v))
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
