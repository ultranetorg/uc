using System.Text;
using RocksDbSharp;

namespace Uccs.Rdn;

public class DomainNameTable : Table<StringId, DomainName>
{
	public new RdnMcv									Mcv => base.Mcv as RdnMcv;
	public static Dictionary<string, HashSet<string>>	Priority = [];

	public DomainNameTable(RdnMcv mcv) : base(mcv, RdnTable.Domain.ToString())
	{
		lock(Priority)
			if(Priority.Count == 0)
			{
				foreach(var tld in DomainName.PriorityTlds)
				{
					foreach(var i in File.ReadLines(Path.Join(mcv.Datapath, tld)))
						(Priority.ContainsKey(tld) ? Priority[tld] : (Priority[tld] = [])).Add(i);
				}
			}
	}
	
	public override DomainName Create()
	{
		return new DomainName(Mcv);
	}

	public StringId GetId(string name)
	{
		return new StringId(Encoding.ASCII.GetBytes(name));
	}
	
 	public DomainName Find(string address)
 	{
		return Find(GetId(address));
 	}

	public virtual DomainName Latest(string name)
	{
		return Latest(GetId(name));
	}
}

public class DomainNameExecution : TableExecution<StringId, DomainName, DomainNameTable>
{
	new DomainNameTable		Table => base.Table as DomainNameTable;
	new RdnExecution		Execution=> base.Execution as RdnExecution;
		
	public DomainNameExecution(RdnExecution execution) : base(execution.Mcv.DomainNames, execution)
	{
	}

	public DomainName Create(string name)
	{
		var d = new DomainName(Execution.Mcv);

		d.Id = Table.GetId(name);

		return Affected[d.Id] = d;
	}

	public DomainName Find(string name)
	{
		var id = Table.GetId(name);
		
		if(Affected.TryGetValue(id, out var a))
			return a.Deleted ? null : a;

		if(Parent != null)
			return (Parent as DomainNameExecution).Find(name);

		if(Execution.Round.DomainNames.Affected.TryGetValue(id, out a))
			return a.Deleted ? null : a;

		return Table.Find(id);
	}

//	public Domain Affect(AutoId id)
//	{
//		var d = Find(name);
//
//		if(d != null)
//			return Affected[d.Id] = d.Clone() as Domain;
//		else
//		{
//			d = new Domain(Execution.Mcv);
//
//			d.Id = LastCreatedId	= new AutoId(Execution.IncrementMetaInt(RdnMetaEntityType.DomainIdCounter));
//			d.Address				= name;
//
//			return Affected[d.Id] = d;
//		}
//	}

}
