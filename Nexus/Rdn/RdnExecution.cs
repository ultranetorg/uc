namespace Uccs.Rdn;

public class RdnExecution : Execution
{
	public new Rdn					Net => base.Net as Rdn;
	public new RdnMcv				Mcv => base.Mcv as RdnMcv;
	public new RdnRound				Round => base.Round as RdnRound;

	public DomainNameExecution		DomainNames;
	public DomainExecution			Domains;
	public ResourceExecution		Resources;
	public NameExecution			UserNames;
	public ResourceNameExecution	ResourceNames;

	public RdnExecution(RdnMcv mcv, RdnRound round, Transaction transaction) : base(mcv, round, transaction)
	{
		DomainNames = new(this);
		Domains = new(this);
		Resources = new(this);
		UserNames = new(this);
		ResourceNames = new(this);
	}

	public override User CreateUser(string name)
	{
		var u = base.CreateUser(name) as RdnUser;
	
		u.Domains = [];
		u.DomainNames = [];

		UserNames.Register(name, u.Id);

		return u;
	}

	public override ITableExecution FindExecution(byte table)
	{
		if(table == Mcv.DomainNames.Id)		return DomainNames;
		if(table == Mcv.Domains.Id)				return Domains;
		if(table == Mcv.Resources.Id)			return Resources;
		if(table == Mcv.UserNames.Id)			return UserNames;
		if(table == Mcv.ResourceNames.Id)		return ResourceNames;

		return base.FindExecution(table);
	}

	public override IBaseTableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Domains.Id == table)			return Domains.Find(id as AutoId) != null ?		(IBaseTableEntry)Domains.Affect(id as AutoId) : null;
		if(Mcv.Resources.Id == table)		return Resources.Find(id as AutoId) != null ?	(IBaseTableEntry)Resources.Affect(id as AutoId) : null;

		return base.Affect(table, id);
	}

	public void PayForName(User user, DomainName address, byte years)
	{
		if(years > 1)
			user.Free = false;

		var now = Time.Days;
		var start = now >= address.Expiration ? now : address.Expiration;

		address.Expiration = (short)(start + Time.FromYears(years).Days);

		if(!user.Free)
		{
			var fee = NameSpacetimeFee(years, address.Id.Text);
			
			user.Spacetime -= fee;
			SpacetimeSpenders.Add(user);
		}
	}

	public static int NameSpacetimeFee(int years, string address)
	{
		var l = Math.Min(address.Length, 10);

		return 10_000_000 * years / (l * l * l * l);
	}

	public void PayOutwardEnergy(IEnergyHolder spender)
	{
		PayEnergy(spender, Net.MigrationEnergyCost);
	}

	//public void PayForForever(int size)
	//{
	//	var s = AffectSigner();
	//
	//	s.Spacetime -= ToBD(size, Uccs.Net.Mcv.Forever);
	//	SpacetimeSpenders.Add(s);
	//}

}
