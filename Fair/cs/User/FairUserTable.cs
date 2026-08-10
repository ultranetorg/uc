using RocksDbSharp;

namespace Uccs.Fair;

public class FairUserTable : UserTable
{
	public new FairMcv		Mcv => base.Mcv as FairMcv;

	public FairUserTable(Mcv chain) : base(chain)
	{
	}

	public override User Create()
	{
		return new FairUser(Mcv);
	}
}
