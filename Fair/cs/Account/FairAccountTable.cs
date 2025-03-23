namespace Uccs.Fair;

public class FairAccountTable : AccountTable
{
	public FairAccountTable(Mcv chain) : base(chain)
	{
	}

	public override Account Create()
	{
		return new FairAccount(Mcv);
	}
}
