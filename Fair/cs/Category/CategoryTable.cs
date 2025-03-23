namespace Uccs.Fair;

public class CategoryTable : Table<Category>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public CategoryTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Category Create()
	{
		return new Category(Mcv);
	}
 }
