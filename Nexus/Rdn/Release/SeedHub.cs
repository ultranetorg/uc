namespace Uccs.Rdn;

public class ReleaseDeclaration : IBinarySerializable
{
	public Urn				Urn { get; set; }	
	public Availability		Availability { get; set; }

	public void Read(Reader reader)
	{
		Urn				= reader.Read<Urn>();
		Availability	= reader.Read<Availability>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Urn);
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

	public IEnumerable<ReleaseDeclarationResult> ProcessIncoming(Endpoint ip, ReleaseDeclaration[] resources)
	{
		foreach(var d in resources)
		{
			var urn = d.Urn;

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
				List<Seed> seeds;

				if(!Releases.TryGetValue(d.Urn, out seeds))
					if(Mcv.Releases.Latest(d.Urn) != null)
						Releases[d.Urn] = seeds = new();
					else
					{
	  					yield return new (urn, DeclarationResult.Rejected);
						continue;
	  				}

				var s = seeds.Find(i => i.Endpoint.Equals(ip));
	
				if(s == null)
				{
					s = new Seed(ip, DateTime.UtcNow, d.Availability);
					seeds.Add(s);
				} 
				else
					s.Arrived = DateTime.UtcNow;

				s.Availability = d.Availability;

				yield return new (urn, DeclarationResult.Accepted);

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
