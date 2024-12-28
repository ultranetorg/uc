namespace Uccs.Rdn.CLI;

public abstract class RdnCommand : McvCommand
{
	public readonly ArgumentType DA			= new ArgumentType("DA",		@"Domain address, a text of [a...z],[0...9] and ""_"" symbols",				[@"demo.application.company"]);
	public readonly ArgumentType RDA		= new ArgumentType("RDA",		@"Root domain address",														[@"ultranet123"]);
	public readonly ArgumentType SDA		= new ArgumentType("SDA",		@"Subdoman only domain address",											[@"demo.application.company"]);
	public readonly ArgumentType RA			= new ArgumentType("RA",		@"Full resource address in form of ""scheme:net/domain/resource"" form",	[@"/company/application (short form)"]);
	public readonly ArgumentType RLA		= new ArgumentType("RLA",		@"Resource release address",												[@""]);
	public readonly ArgumentType YEARS		= new ArgumentType("YEARS",		@"Number of years",															[@"5"]);
	public readonly ArgumentType URL		= new ArgumentType("URL",		@"A text string that meets URL specifications",								[@"htttp://domain.com"]);
	public readonly ArgumentType WTLD		= new ArgumentType("WTLD",		@"Web top-level domain",													[@"domain.com"]);

	protected RdnCli			Program;
	protected override Type[]	TypesForExpanding => [typeof(IEnumerable<Dependency>), 
													  typeof(IEnumerable<AnalyzerResult>), 
													  typeof(Resource), 
													  typeof(VersionManifest)];
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
						return new ResourceData(t, new Consil{	Analyzers = d.Get<string>("analyzers").Split(',').Select(AccountAddress.Parse).ToArray(),  
																PerByteBYFee = d.Get<long>("pbstf") });
					
					if(cnt == ContentType.Rdn_Analysis)
						return new ResourceData(t, new Analysis{Release		= Urr.Parse(d.Get<string>("release")), 
																ECPayment	= [new (Time.Zero, d.Get<long>("expayment"))],
																BYPayment	= d.Get<long>("stpayment"),
																Consil		= d.Get<Ura>("consil")});
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
