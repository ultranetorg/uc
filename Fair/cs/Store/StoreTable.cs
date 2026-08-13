using RocksDbSharp;

namespace Uccs.Fair;

public class StoreTable : Table<AutoId, Store>
{
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public StoreTable(FairMcv mcv) : base(mcv, FairTable.Store.ToString())
	{
	}
	
	public override Store Create()
	{
		return new Store(Mcv);
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var cl in Clusters)
			foreach(var b in cl.Buckets)
				foreach(var i in b.Entries.Where(i => i.Name != null))
				{
					var w = e.Names.Affect(NameTable.GetId(i.Name));

					w.Entity = new EntityField<EntityTextField> {Id = i.Id, Field = EntityTextField.StoreName};
				}

		Mcv.Names.Commit(batch, e.Names.Affected.Values, null, lastincommit);

		e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Stores.GraphEntities)
		{
			e.StoreTitles.Index(i.Title, null, i.Id);
		}
	
		Mcv.StoreTitles.Commit(batch, e.StoreTitles.Affected.Values, null, lastincommit);
	}
}

public class StoreExecution : TableExecution<AutoId, Store, StoreTable>
{
	public StoreExecution(FairExecution execution) : base(execution.Mcv.Stores, execution)
	{
	}

	public Store Create(User signer)
	{
		Execution.IncrementMetaInt(FairMetaEntityType.StoreCount);

		var s = Table.Create();
		
		s.Id = LastCreatedId = new AutoId(Execution.IncrementMetaInt(FairMetaEntityType.StoreIdCounter));
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
