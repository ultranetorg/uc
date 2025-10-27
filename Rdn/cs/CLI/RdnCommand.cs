namespace Uccs.Rdn.CLI;

public abstract class RdnCommand : McvCommand
{
	public static readonly ArgumentType DA		= new ArgumentType("DA",	@"Domain address, a text of [a...z],[0...9] and ""_"" symbols",				[@"demo.application.company"]);
	public static readonly ArgumentType RDA		= new ArgumentType("RDA",	@"Root domain address",														[@"ultranet123"]);
	public static readonly ArgumentType SDA		= new ArgumentType("SDA",	@"Subdoman address",														[@"application.company"]);
	public static readonly ArgumentType RA		= new ArgumentType("RA",	@"Full resource address in form of ""scheme:net/domain/resource"" form",	[@"/company/application (short form)"]);
	public static readonly ArgumentType YEARS	= new ArgumentType("YEARS",	@"Number of years",															[@"5"]);
	public static readonly ArgumentType TLD		= new ArgumentType("TLD",	@"Web top-level domain",													[@"com"]);
	public static readonly ArgumentType RZA		= new ArgumentType("RZA",	@"Release address",															[$@"{UrrScheme.Urrh}:F371BC4A311F2B009EEF952DD83CA80E2B60026C8E935592D0F9C308453C813E"]);

	protected RdnCli			Program;

	static RdnCommand()
	{
		try
		{
			var p = Console.KeyAvailable;
			ConsoleAvailable = true;
		}
		catch(Exception)
		{
			ConsoleAvailable = false;
		}
	}

	protected RdnCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Program = program;

		if(flow != null)
		{
			Flow.Log.TypesForExpanding.AddRange([typeof(IEnumerable<Dependency>), 
												 typeof(IEnumerable<AnalyzerResult>), 
												 typeof(Resource), 
												 typeof(VersionManifest)]);
		}
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
		return text.Contains('/') ? Ppc(new ResourceRequest(Ura.Parse(text))).Resource.Id
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
				var cnt = GetString("data/type", false) is string a ? Enum.Parse<ContentType>(a) : ContentType.Unknown;
				var t = new DataType(ctl, cnt);

				if(ctl == DataType.Data)
				{	
					if(cnt == ContentType.Unknown)
						return new ResourceData(t, d.Get<string>("hex").FromHex());
			
					if(cnt == ContentType.Rdn_Consil)
						return new ResourceData(t, new Consil  {Analyzers = d.Get<string>("analyzers").Split(',').Select(AccountAddress.Parse).ToArray(),  
																SizeEnergyFeeMinimum = d.Get<long>("sefm"),
																ResultEnergyFeeMinimum = d.Get<long>("refm"),
																ResultSpacetimeFeeMinimum = d.Get<long>("rstfm")});
					
					if(cnt == ContentType.Rdn_Analysis)
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
