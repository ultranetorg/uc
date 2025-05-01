namespace Uccs.Rdn;

public class RdnExecution : Execution
{
	public new Rdn				Net => base.Net as Rdn;
	public new RdnMcv			Mcv => base.Mcv as RdnMcv;
	public new RdnRound			Round => base.Round as RdnRound;

	public DomainExecution		Domains;
	public ResourceExecution	Resources;

	public RdnExecution(RdnMcv mcv, RdnRound round, Transaction transaction) : base(mcv, round, transaction)
	{
		Domains = new(this);
		Resources = new(this);
	}

	public override ITableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Domains.Id == table)		return Domains.Find(id as AutoId) != null ? Domains.Affect(id as AutoId) : null;
		if(Mcv.Resources.Id == table)	return Resources.Find(id as AutoId) != null ? Resources.Affect(id as AutoId) : null;

		return base.Affect(table, id);
	}
}
