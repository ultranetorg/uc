namespace Uccs.Fair;

public class CategoryTable : Table<CategoryEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public CategoryTable(FairMcv rds) : base(rds)
	{
	}
	
	public override CategoryEntry Create()
	{
		return new CategoryEntry(Mcv);
	}
 }
