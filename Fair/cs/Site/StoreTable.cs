using RocksDbSharp;

namespace Uccs.Fair;

public class StoreTable : Table<AutoId, Store>
{
	public override string			Name => FairTable.Store.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public StoreTable(FairMcv mcv) : base(mcv)
	{
	}
	
	public override Store Create()
	{
		return new Store(Mcv);
	}
}

public class StoreExecution : TableExecution<AutoId, Store>
{
	public StoreExecution(FairExecution execution) : base(execution.Mcv.Stores, execution)
	{
	}

	public Store Create(User signer)
	{
		Execution.IncrementCount((int)FairMetaEntityType.StoreCount);

		var b = UserTable.KeyToBucket(signer.Name);
		int e = Execution.GetNextEid(Table, b);

		var s = Table.Create();
		
		s.Id = LastCreatedId = new AutoId(b, e);
		s.Categories = [];
		s.Moderators = [];
		s.Publishers = [];
		s.Users = [];
		s.Proposals = [];
		s.UnpublishedPublications = [];
		s.ChangedPublications = [];
		s.Files = [];
		
		return Affected[s.Id] = s;
	}

	public override Store Affect(AutoId id)
	{
		var e = base.Affect(id);

		Execution.TransferEnergyIfNeeded(e);

		return e;
	}

}
