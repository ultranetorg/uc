namespace Uccs.Smp;

public class SmpAccountTable : AccountTable
{
	public SmpAccountTable(Mcv chain) : base(chain)
	{
	}

	public override AccountEntry Create()
	{
		return new SmpAccountEntry(Mcv);
	}
}
