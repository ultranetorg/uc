namespace Uccs.Fair;

public class FairAccountTable : AccountTable
{
	public FairAccountTable(Mcv chain) : base(chain)
	{
	}

	public override AccountEntry Create()
	{
		return new FairAccountEntry(Mcv);
	}
}
