using System.Text;

namespace Uccs.Fair;

public class ReviewTable : Table<AutoId, Review>
{
	public override string			Name => FairTable.Review.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ReviewTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Review Create()
	{
		return new Review(Mcv);
	}
 }

public class ReviewExecution : TableExecution<AutoId, Review>
{
	new FairExecution Execution => base.Execution as FairExecution;

	public ReviewExecution(FairExecution execution) : base(execution.Mcv.Reviews, execution)
	{
	}

	public Review Create(AutoId publication)
	{
		Execution.IncrementCount((int)FairMetaEntityType.ReviewsCount);

		int e = Execution.GetNextEid(Table, publication.B);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(publication.B, e);

		return Affected[a.Id] = a;
	}
		
	public void Delete(Site site, AutoId id)
	{
		var v = Execution.Reviews.Affect(id);
		
		v.Deleted = true;
		
		Execution.Free(site, site, Encoding.UTF8.GetByteCount(v.Text));
		
		if(v.TextNew.Length > 0)
		{
			Execution.Free(site, site, Encoding.UTF8.GetByteCount(v.TextNew));
		}

		Execution.Free(site, site, Execution.Net.EntityLength);
	}
}
