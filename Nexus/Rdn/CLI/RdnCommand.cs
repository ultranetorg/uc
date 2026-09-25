namespace Uccs.Rdn.CLI;

public abstract class RdnCommand : McvCommand
{
	public static readonly ArgumentType		DN		= new (nameof(DN),	"Domain address, a text of [a..z 0..9 _ .] symbols",		["company", "application.company", "x_y_z.application.company"]);
	public static readonly ArgumentType		DRN		= new (nameof(DRN),	"Domain root name, a text of [a..z 0..9 _] symbols",		["ultranetorg", "company", "a_123"]);
	public static readonly ArgumentType		DCP		= new (nameof(DCP),	"Domain child policy",										Enum.GetNames<OwnershipPolicy>().Where(i => i != OwnershipPolicy.None.ToString()).ToArray());
	public static readonly ArgumentType		TLD		= new (nameof(TLD),	"Top-level  web domain",									DomainName.PriorityTlds);
	public static readonly ArgumentType		RA		= new (nameof(RA),	$"Resource address including domain",						[@"/company/application", "rdn/author/product"]);
	public static readonly ArgumentType		RLT		= new (nameof(RLT),	"Resource link type",										Enum.GetNames<ResourceLinkType>().Where(i => i != ResourceLinkType.None.ToString()).ToArray());
	public static readonly ArgumentType		RZA		= new (nameof(RZA),	"Release address",											[$"{UrnScheme.Hcid.ToString().ToLower()}:F371BC4A311F2B009EEF952DD83CA80E2B60026C8E935592D0F9C308453C813E"]);

	new protected RdnCli					Cli => base.Cli as RdnCli;

	protected AutoId ResourceId
	{
		get
		{
			if(Has(IdKeyword))
				return GetAutoId(IdKeyword);
			else if(Has(AddressKeyword))
				return Ppc(new ResourceByAddressPpc(Ura.Parse(Address))).Resource.Id;
			else
				throw new SyntaxException("Neither 'id' nor 'name' arguments provided");

		}
	}

	protected RdnCommand(RdnCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
		Flow.Log?.TypesForExpanding.AddRange([typeof(IEnumerable<AnalyzerReport>), 
											  typeof(Resource)]);
	}

	protected RdnCommand()
	{
	}

	protected Ura GetResourceAddress(string paramenter, bool mandatory = true)
	{
		if(Has(paramenter))
			return Ura.Parse(GetString(paramenter));
		else
			if(mandatory)
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
			else
				return null;
	}

	protected AutoId GetResourceId(string text)
	{
		return text.Contains('/') ? Ppc(new ResourceByAddressPpc(Ura.Parse(text))).Resource.Id
									:
									AutoId.Parse(text);
	}

	protected Urn GetReleaseAddress(string paramenter, bool mandatory = true)
	{
		if(Has(paramenter))
			return Urn.Parse(GetString(paramenter));
		else
			if(mandatory)
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
			else
				return null;
	}

	protected ResourceData GetData()
	{
		var d = One("data");

		if(d != null)
		{
			if(d.Nodes.Any())
			{
				Meaning m;
				ContentType c;

				var t = d.Get<string>();
				var i = t.IndexOf('/');
			
				if(i == -1)
				{	
					m = Enum.Parse<Meaning>(t, true);
					c = ContentType.Undefined;
				}
				else
				{	
					m = Enum.Parse<Meaning>(t.Substring(0, i), true); 
					c = Enum.Parse<ContentType>(t.AsSpan(i + 1, t.Length - i - 1), true);
				}

				switch(m)
				{	
					case Meaning.Raw :
						return new ResourceData(m, d.Get<string>("hex").FromHex());
			
					case Meaning.Ampp_Council :
						return new ResourceData(m,	new Consil
													{
														Analyzers					= d.Get<string>("analyzers").Split(',').Select(PublicKey.Parse).ToArray(),  
														SizeEnergyFeeMinimum		= d.Get<long>("sefm"),
														ResultEnergyFeeMinimum		= d.Get<long>("refm"),
														ResultSpacetimeFeeMinimum	= d.Get<long>("rstfm")
													});
					
					case Meaning.Ampp_Analysis :
						return new ResourceData(m,	new Analysis
													{
														Release			= Urn.Parse(d.Get<string>("release")), 
														EnergyReward	= d.Get<long>("ereward"),
														SpacetimeReward	= d.Get<long>("streward"),
														Consil			= GetResourceId(d.Get<string>("consil"))
													});
					
					case Meaning.DnsRecord :
						return new ResourceData(m,	new DnsRecord
													{
														Type	= d.GetEnum<DnsRecordType>("type"), 
														Value	= d.Get<string>("value"),
														TTL		= d.Get("ttl", 3600)
													});
					case Meaning.Rex_File:
					case Meaning.Rex_Directory :
						return new ResourceData(m, Urn.Parse(d.Get<string>("address")));
				}
			}
			else if(d.Value == null) /// Type without value means remove data
				return null;

			throw new SyntaxException("Unknown or missing meaning/type");
		}

		return null;
	}
}
