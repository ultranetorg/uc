using System.Text;

namespace Uccs.Fair;

public class ReviewTable : Table<AutoId, Review>
{
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ReviewTable(FairMcv rds) : base(rds, FairTable.Review.ToString())
	{
	}
	
	public override Review Create()
	{
		return new Review(Mcv);
	}
 }

public class ReviewExecution : TableExecution<AutoId, Review, ReviewTable>
{
	new FairExecution Execution => base.Execution as FairExecution;

	public ReviewExecution(FairExecution execution) : base(execution.Mcv.Reviews, execution)
	{
	}

	public Review Create(AutoId publication)
	{
		Execution.IncrementMetaInt(FairMetaEntityType.ReviewsCount);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(Execution.IncrementMetaInt(FairMetaEntityType.ReviewsIdCounter));

		return Affected[a.Id] = a;
	}
		
	public void Delete(Store store, AutoId id)
	{
		var v = Execution.Reviews.Affect(id);
		
		v.Deleted = true;
		
		var u = Execution.AffectUser(v.Creator);
		u.Reviews = u.Reviews.Remove(v.Id);
		
		Execution.Free(store, store, Encoding.UTF8.GetByteCount(v.Text));
		Execution.Free(store, store, Execution.Net.EntityLength);
	}
}
