using System.Text;

namespace Uccs.Rdn;

public class DomainTable : Table<AutoId, Domain>
{
	public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();

	public int						KeyToBid(string domain) => EntityId.BytesToBucket(Encoding.UTF8.GetBytes(domain.PadRight(3, '\0'), 0, 3));

	public DomainTable(RdnMcv rds) : base(rds)
	{
	}
	
	public override Domain Create()
	{
		return new Domain(Mcv);
	}
	
 	public Domain Find(string name, int ridmax)
 	{
 		foreach(var r in Tail.Where(i => i.Id <= ridmax))
			if(r.AffectedDomains.Values.FirstOrDefault(i => i.Address == name) is Domain d && !d.Deleted)
				return d;
 		
		var bid = KeyToBid(name);

		return FindBucket(bid)?.Entries.Find(i => i.Address == name);
 	}
}
