namespace Uccs.Rdn.CLI;

public abstract class RdnCommand : McvCommand
{
	public static readonly ArgumentType DA		= new ("DA",	@"Domain address, a text of [a...z],[0...9] and ""_"" symbols",				[@"demo.application.company"]);
	public static readonly ArgumentType RDA		= new ("RDA",	@"Root domain address",														[@"ultranet123"]);
	public static readonly ArgumentType SDA		= new ("SDA",	@"Subdoman address",														[@"application.company"]);
	public static readonly ArgumentType DCP		= new ("DCP",	@"Domain child address",													[DomainChildPolicy.FullFreedom.ToString()]);
	public static readonly ArgumentType RA		= new ("RA",	@"Full resource address in form of ""scheme:net/domain/resource"" form",	[@"rdn/company/application", "/author/product"]);
	public static readonly ArgumentType TLD		= new ("TLD",	@"Web top-level domain",													[@"com"]);
	public static readonly ArgumentType RZA		= new ("RZA",	@"Release address",															[$@"{UrrScheme.Urrh}:F371BC4A311F2B009EEF952DD83CA80E2B60026C8E935592D0F9C308453C813E"]);
	public static readonly ArgumentType LT		= new ("RLT",	@"Resource link type",														[ResourceLinkType.Hierarchy.ToString()]);

	protected RdnCli			Program;

	protected RdnCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Program = program;
	
		Flow.Log?.TypesForExpanding.AddRange([typeof(IEnumerable<AnalyzerReport>), 
											  typeof(Resource)]);
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
		return text.Contains('/') ? Ppc(new ResourcePpc(Ura.Parse(text))).Resource.Id
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
				var ctl = DataType.Parse(GetString("data"));
				var cnt = GetEnum("data/type", ContentType.Unknown);
				var t = new DataType(ctl, cnt);

				if(ctl == DataType.Data)
				{	
					if(cnt == ContentType.Unknown)
						return new ResourceData(t, d.Get<string>("hex").FromHex());
			
					if(cnt == ContentType.Ampp_Consil)
						return new ResourceData(t, new Consil  {Analyzers = d.Get<string>("analyzers").Split(',').Select(AccountAddress.Parse).ToArray(),  
																SizeEnergyFeeMinimum = d.Get<long>("sefm"),
																ResultEnergyFeeMinimum = d.Get<long>("refm"),
																ResultSpacetimeFeeMinimum = d.Get<long>("rstfm")});
					
					if(cnt == ContentType.Ampp_Analysis)
						return new ResourceData(t, new Analysis{Release			= Urr.Parse(d.Get<string>("release")), 
																EnergyReward	= d.Get<long>("ereward"),
																SpacetimeReward	= d.Get<long>("streward"),
																Consil			= GetResourceId(d.Get<string>("consil"))});
				}
				else
				{
					if(	ctl == DataType.File ||
						ctl == DataType.Directory)
						return new ResourceData(t, Urr.Parse(d.Get<string>("address")));
				}

				throw new SyntaxException("Unknown type");
			}
			else if(d.Value != null)
				return new ResourceData(new BinaryReader(new MemoryStream(GetBytes("data"))));
		}

		return null;
	}
}
