using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public class Seed
	{
		public IPAddress			IP;
		public DateTime				Arrived;
		public Availability			Availability;

		public Seed(IPAddress iP, DateTime arrived, Availability availability)
		{
			IP = iP;
			Arrived = arrived;
			Availability = availability;
		}

		public override string ToString()
		{
			return IP.ToString();
		}
	}

	public class SeedHub
	{
		public const int											SeedsPerReleaseMax = 100;
		public const int											SeedsPerRequestMax = 50;
		public Dictionary<ReleaseAddress, List<Seed>>				Releases = new ();
		public Dictionary<ResourceAddress, List<ReleaseAddress>>	Resources = new ();
		public object												Lock = new object();
		Sun															Sun;

		public SeedHub(Sun sun)
		{
			Sun = sun;
		}

		public List<ReleaseDeclarationResult> ProcessIncoming(IPAddress ip, ResourceDeclaration[] resources)
		{
			var results = new List<ReleaseDeclarationResult>();

			foreach(var rsd in resources)
			{
				foreach(var rzd in rsd.Releases)
				{
					lock(Sun.Lock)
					{ 
						if(!Sun.NextVoteMembers.OrderByNearest(rzd.Address.Hash).Take(ResourceHub.MembersPerDeclaration).Any(i => Sun.Settings.Generators.Contains(i.Account)))
						{
							results.Add(new (rzd.Address, DeclarationResult.NotNearest));
							continue;
						}
					}

					if(Releases.TryGetValue(rzd.Address, out var ss))
					{
						var s = ss.Find(i => i.IP.Equals(ip));
	
						if(s == null)
						{
							s = new Seed(ip, DateTime.UtcNow, rzd.Availability);
							ss.Add(s);
						} 
						else
						{
							s.Arrived = DateTime.UtcNow;
						}
						
						s.Availability = rzd.Availability;
					}
					else
					{
						lock(Sun.Lock)
						{
							if(rzd.Address is HashAddress da)
							{
								var z = Sun.Mcv.Releases.Find(rzd.Address, Sun.Mcv.LastConfirmedRound.Id);
	
								if(z == null)
								{
									var e = Sun.Mcv.Authors.FindResource(rsd.Resource, Sun.Mcv.LastConfirmedRound.Id);

									if(e?.Data == null || e.Data.Interpretation is HashAddress ha && ha != da)
									{
										results.Add(new (rzd.Address, DeclarationResult.Rejected));
										continue;
									}
								}
							}
							else if(rzd.Address is ProvingAddress pa)
							{
								var ea = Sun.Mcv.Authors.Find(rsd.Resource.Author, Sun.Mcv.LastConfirmedRound.Id);
	
								if(!pa.Valid(Sun.Zone.Cryptography, rsd.Resource, ea.Owner))
								{
									results.Add(new (rzd.Address, DeclarationResult.Rejected));
									continue;
								}

								var s = (Resources.TryGetValue(rsd.Resource, out var l) ? l : Resources[rsd.Resource] = new());
								
								s.Add(rzd.Address);

								if(s.Count > 50)
								{
									s.RemoveAt(0);
								}
							}
						}

						ss = Releases[rzd.Address] = new () {new Seed(ip, DateTime.UtcNow, rzd.Availability)};
					}
									
					results.Add(new (rzd.Address, DeclarationResult.Accepted));
				
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
