using System.Net;

namespace Uccs.Rdn
{
	public class ResourceDeclaration
	{
		public ResourceId		Resource { get; set; }	
		public Urr				Release { get; set; }	
		public byte[]			Hash { get; set; }
		public Availability		Availability { get; set; }	
	}

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
		public const int					SeedsPerReleaseMax = 100;
		public const int					SeedsPerRequestMax = 50;
		public Dictionary<Urr, List<Seed>>	Releases = [];
		public object						Lock = new ();
		RdnMcv								Node;

		public SeedHub(RdnMcv sun)
		{
			Node = sun;
		}

		public IEnumerable<ReleaseDeclarationResult> ProcessIncoming(IPAddress ip, ResourceDeclaration[] resources)
		{
			foreach(var rsd in resources)
			{
				var rzd = rsd.Release;
				//foreach(var rzd in rsd.Releases)
				{
					lock(Node.Lock)
					{ 
						if(!Node.NextVoteMembers.OrderByNearest(rzd.MemberOrderKey).Take(ResourceHub.MembersPerDeclaration).Any(i => Node.Settings.Generators.Contains(i.Account)))
						{
							yield return new (rzd, DeclarationResult.NotNearest);
							continue;
						}
					}

					lock(Lock)
					{
						bool valid()
						{
							if(rzd is Urrh dh)
							{
								var r = Node.Domains.FindResource(rsd.Resource, Node.LastConfirmedRound.Id);
	
								if(r?.Data?.Interpretation is Urrh ha && ha == dh)
								{
									return true;
								}
							}
							///else if(rzd is Urrsd sdp)
							///{
							///	var d = Node.Domains.Find(rsd.Resource.DomainId, Node.LastConfirmedRound.Id);
							///	var o = Node.Accounts.Find(d.Owner, Node.LastConfirmedRound.Id);
							///
							///	if(!sdp.Prove(Node.Zone.Cryptography, o.Address, rsd.Hash))
							///	{
							///		return false;
							///	}
							///
							///}

							return false;
						}

						List<Seed> seeds;
	
	// 					if(!Resources.TryGetValue(rsd.Resource, out var releases))
	// 						if(valid())
	// 							Resources[rsd.Resource] = releases = new ();
	// 						else
	// 						{
	// 	  						yield return new (rzd, DeclarationResult.Rejected);
	// 							continue;
	// 	  					}
	
						if(!Releases.TryGetValue(rsd.Release, out seeds))
							if(valid())
								Releases[rsd.Release] = seeds = new();
							else
							{
		  						yield return new (rzd, DeclarationResult.Rejected);
								continue;
		  					}
	
						var s = seeds.Find(i => i.IP.Equals(ip));
		
						if(s == null)
						{
							s = new Seed(ip, DateTime.UtcNow, rsd.Availability);
							seeds.Add(s);
						} 
						else
							s.Arrived = DateTime.UtcNow;
	
						s.Availability = rsd.Availability;
	
						yield return new (rzd, DeclarationResult.Accepted);
					
						///if(releases.Count > 50)
						///{
						///	releases.RemoveAt(0);
						///}
	
						if(seeds.Count > SeedsPerReleaseMax)
						{
							seeds.RemoveRange(0, seeds.Count - SeedsPerReleaseMax);
						}
					}
				}
			}
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
