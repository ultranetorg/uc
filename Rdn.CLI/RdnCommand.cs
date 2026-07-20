namespace Uccs.Rdn.CLI;

public abstract class RdnCommand : McvCommand
{
	public static readonly ArgumentType		DA		= new (nameof(DA),	"Domain address, a text of [a..z 0..9 _ .] symbols",		["company", "application.company", "x_y_z.application.company"]);
	public static readonly ArgumentType		DN		= new (nameof(DN),	"Domain name, a text of [a..z 0..9 _] symbols",				["ultranetorg", "company", "a123"]);
	public static readonly ArgumentType		DCP		= new (nameof(DCP),	"Domain child policy",										Enum.GetNames<DomainChildPolicy>().Where(i => i != DomainChildPolicy.None.ToString()).ToArray());
	public static readonly ArgumentType		TLD		= new (nameof(TLD),	"Top-level  web domain",									Domain.PriorityTlds);
	public static readonly ArgumentType		RA		= new (nameof(RA),	$"Resource address including domain",						[@"/company/application", "rdn/author/product"]);
	public static readonly ArgumentType		RLT		= new (nameof(RLT),	"Resource link type",										Enum.GetNames<ResourceLinkType>().Where(i => i != ResourceLinkType.None.ToString()).ToArray());
	public static readonly ArgumentType		RZA		= new (nameof(RZA),	"Release address",											[$"{UrrScheme.Rrrh.ToString().ToLower()}:F371BC4A311F2B009EEF952DD83CA80E2B60026C8E935592D0F9C308453C813E"]);

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

	protected Urr GetReleaseAddress(string paramenter, bool mandatory = true)
	{
		if(Has(paramenter))
			return Urr.Parse(GetString(paramenter));
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
				var t = DataType.Parse(d.Get<string>());

				if(t.Meaning == DataType.Self)
				{	
					if(t.Content == ContentType.Unknown)
						return new ResourceData(t, d.Get<string>("hex").FromHex());
			
					if(t.Content == ContentType.Ampp_Council)
						return new ResourceData(t,	new Consil
													{
														Analyzers = d.Get<string>("analyzers").Split(',').Select(PublicKey.Parse).ToArray(),  
														SizeEnergyFeeMinimum = d.Get<long>("sefm"),
														ResultEnergyFeeMinimum = d.Get<long>("refm"),
														ResultSpacetimeFeeMinimum = d.Get<long>("rstfm")
													});
					
					if(t.Content == ContentType.Ampp_Analysis)
						return new ResourceData(t,	new Analysis
													{
														Release			= Urr.Parse(d.Get<string>("release")), 
														EnergyReward	= d.Get<long>("ereward"),
														SpacetimeReward	= d.Get<long>("streward"),
														Consil			= GetResourceId(d.Get<string>("consil"))
													});
					
					if(t.Content == ContentType.DnsRecord)
						return new ResourceData(t,	new DnsRecord
													{
														Type	= d.GetEnum<DnsRecordType>("type"), 
														Value	= d.Get<string>("value"),
														TTL		= d.Get("ttl", 3600)
													});
				}
				else
				{
					if(	t.Meaning == DataType.File ||
						t.Meaning == DataType.Directory)
						return new ResourceData(t, Urr.Parse(d.Get<string>("address")));
				}
			}
			else if(d.Value == null)
				return null;

			throw new SyntaxException("Unknown or missing meaning/type");
		}

		return null;
	}
}
