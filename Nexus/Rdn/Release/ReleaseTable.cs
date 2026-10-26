using RocksDbSharp;

namespace Uccs.Rdn;

public class ReleaseTable : Table<Urn, Release>
{
	public new RdnMcv		Mcv => base.Mcv as RdnMcv;

	public ReleaseTable(RdnMcv rds) : base(rds, RdnTable.Release.ToString())
	{
	}
	
	public override Release Create()
	{
		return new Release(Mcv);
	}
}

public class ReleaseExecution : TableExecution<Urn, Release, ReleaseTable>
{
	new ReleaseTable		Table => base.Table as ReleaseTable;
	new RdnExecution		Execution=> base.Execution as RdnExecution;
		
	public ReleaseExecution(RdnExecution execution) : base(execution.Mcv.Releases, execution)
	{
	}
	
	public Release Create(Urn urn)
	{
		var d = new Release(Execution.Mcv);

		return Affected[urn] = d;
	}

}
