﻿namespace Uccs.Fair;

public class DisputeTable : Table<AutoId, Dispute>
{
	public override string			Name => FairTable.Dispute.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public DisputeTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Dispute Create()
	{
		return new Dispute(Mcv);
	}
}
public class DisputeExecution : TableExecution<AutoId, Dispute>
{
	public DisputeExecution(FairExecution execution) : base(execution.Mcv.Disputes, execution)
	{
	}

	public Dispute Create(Site site)
	{
		Execution.IncrementCount((int)FairMetaEntityType.DisputesCount);

		int e = Execution.GetNextEid(Table, site.Id.B);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(site.Id.B, e);
		a.Yes = [];
		a.No = [];
		a.Abs = [];
		a.Comments = [];

		LastCreatedId = a.Id;

		return Affected[a.Id] = a;
	}
}