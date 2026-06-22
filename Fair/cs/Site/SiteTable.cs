using RocksDbSharp;

namespace Uccs.Fair;

public class SiteTable : Table<AutoId, Site>
{
	public override string			Name => FairTable.Site.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public SiteTable(FairMcv mcv) : base(mcv)
	{
	}
	
	public override Site Create()
	{
		return new Site(Mcv);
	}
}

public class SiteExecution : TableExecution<AutoId, Site>
{
	public SiteExecution(FairExecution execution) : base(execution.Mcv.Sites, execution)
	{
	}

	public Site Create(User signer)
	{
		Execution.IncrementCount((int)FairMetaEntityType.SitesCount);

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

	public override Site Affect(AutoId id)
	{
		var e = base.Affect(id);

		Execution.TransferEnergyIfNeeded(e);

		return e;
	}

}
