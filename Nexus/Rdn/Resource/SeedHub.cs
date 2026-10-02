using System.Net;

namespace Uccs.Rdn;

public class ResourceDeclaration : IBinarySerializable
{
	public AutoId			Resource { get; set; }	
	public Urn				Urn { get; set; }	
	public Availability		Availability { get; set; }

	public void Read(Reader reader)
	{
		Resource = reader.Read<AutoId>();
		Urn = reader.ReadVirtual<Urn>();
		Availability = reader.Read<Availability>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Resource);
		writer.WriteVirtual(Urn);
		writer.Write(Availability);
	}
}

public class Seed
{
	public Endpoint				Endpoint;
	public DateTime				Arrived;
	public Availability			Availability;

	public Seed(Endpoint iP, DateTime arrived, Availability availability)
	{
		Endpoint = iP;
		Arrived = arrived;
		Availability = availability;
	}

	public override string ToString()
	{
		return Endpoint.ToString();
	}
}

public class SeedHub
{
	public const int					SeedsPerReleaseMax = 100;
	public const int					SeedsPerRequestMax = 50;
	public Dictionary<Urn, List<Seed>>	Releases = [];
	public object						Lock = new ();
	RdnMcv								Mcv;

	public SeedHub(RdnMcv mcv)
	{
		Mcv = mcv;
	}

	public IEnumerable<ReleaseDeclarationResult> ProcessIncoming(Endpoint ip, ResourceDeclaration[] resources)
	{
		foreach(var rsd in resources)
		{
			var urn = rsd.Urn;

			lock(Mcv.Lock)
			{ 
				if(!Mcv.NextVotingRound.Senders.OrderByHash(i => i.Generator.Raw, urn.MemberOrderKey).Take(ResourceHub.MembersPerDeclaration).Any(i => Mcv.Settings.Memberships.Any(g => g.GeneratorId == i.Generator)))
				{
					yield return new (urn, DeclarationResult.NotNearest);
					continue;
				}
			}

			lock(Lock)
			{
				bool valid()
				{
					var r = Mcv.Resources.Latest(rsd.Resource);
	
					if(r?.Data?.IsCexInvolved(out var n) ?? false && n == urn)
						return true;
					else
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

				if(!Releases.TryGetValue(rsd.Urn, out seeds))
					if(valid())
						Releases[rsd.Urn] = seeds = new();
					else
					{
	  					yield return new (urn, DeclarationResult.Rejected);
						continue;
	  				}

				var s = seeds.Find(i => i.Endpoint.Equals(ip));
	
				if(s == null)
				{
					s = new Seed(ip, DateTime.UtcNow, rsd.Availability);
					seeds.Add(s);
				} 
				else
					s.Arrived = DateTime.UtcNow;

				s.Availability = rsd.Availability;

				yield return new (urn, DeclarationResult.Accepted);
				
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

 	public Endpoint[] Locate(LocateReleasePpc request)
 	{
 		if(Releases.TryGetValue(request.Address, out var v))
 			return v.OrderByDescending(i => i.Arrived).Take(Math.Min(request.Count, SeedsPerRequestMax)).Select(i => i.Endpoint).ToArray();
 		else
 			return [];
 	}
}
