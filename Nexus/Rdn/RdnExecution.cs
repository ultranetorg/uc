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

	public override ITableExecution FindExecution(byte table)
	{
		if(table == Mcv.Domains.Id)		return Domains;
		if(table == Mcv.Resources.Id)	return Resources;

		return base.FindExecution(table);
	}

	public override ITableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Domains.Id == table)		return Domains.Find(id as AutoId) != null ? Domains.Affect(id as AutoId) : null;
		if(Mcv.Resources.Id == table)	return Resources.Find(id as AutoId) != null ? Resources.Affect(id as AutoId) : null;

		return base.Affect(table, id);
	}

	public void PayForName(string address, int years)
	{
		var fee = NameFee(years, address);
		
		var s = AffectSigner();

		s.Spacetime -= fee;
		SpacetimeSpenders.Add(s);
	}

	public static int NameFee(int years, string address)
	{
		var l = Math.Min(address.Length, 10);

		return 10_000_000 * years / (l * l * l * l);
	}

	//public void PayForForever(int size)
	//{
	//	var s = AffectSigner();
	//
	//	s.Spacetime -= ToBD(size, Uccs.Net.Mcv.Forever);
	//	SpacetimeSpenders.Add(s);
	//}
}
