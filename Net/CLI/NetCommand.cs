using System.Reflection;

namespace Uccs.Net;

public abstract class NetCommand : Command
{
	public static readonly ArgumentType		AA			= new (nameof(AA),			"Account address, in Bech32 form",										["plsar7tfrq83mvfaw0m6u0l9jmsxhkjvk5zzanaqrhavfygl5w2sq3cyyk9btes", "gya27t2jsxdfa3gdywck9cdtwe6lhc59qwwrq2jdq6plz3k9hhjsqx0yuc6btes", "uh39t57ujhn4skcdhkg24mavdp4jsghtvv0uxeg2596et7h6vmnsqrntezqbtes", "vlrvxjhwz96gckyk9etxft3980agmy8s38u45gscp93ajk8rje5qqkgeww3btes"]);
	public static readonly ArgumentType		COMMAND		= new (nameof(COMMAND),		"CLI command string",													["node peers"]);
	public static readonly ArgumentType		EID			= new (nameof(EID),			"Entity Id",															[new AutoId(1111, 22), new AutoId(12345,6789), new AutoId(987, 6543321)]);
	public static readonly ArgumentType		IP			= new (nameof(IP),			"IP Address",															["123.234.55.66"]);
	public static readonly ArgumentType		INT			= new (nameof(INT),			"Positive integer number",												["324552", "1000"]);
	public static readonly ArgumentType		HEX			= new (nameof(HEX),			"Array of bytes in form of hexadecimal string",							["0105BCE1C336874FBEBE40D2510EC035D0251FE855399EAD76E22BD18E2EBC6E37"]);
	public static readonly ArgumentType		HOST		= new (nameof(HOST),		"Host name",															["1.2.3.4"]);
	public static readonly ArgumentType		PASSWORD	= new (nameof(PASSWORD),	"Text string of any no. chars and longer than 1 char",					["MyStrongSecret!@#$%^&*()"]);
	public static readonly ArgumentType		PATH		= new (nameof(PATH),		"A path to local file or directory in native format",					[@"C:\User\readme.txt", @"C:\User\Admin", @"D:\Documents"]);
	public static readonly ArgumentType		FILEPATH	= new (nameof(FILEPATH),	"A text string of the local file path in its native format",			[@"C:\User\file", @"D:\image.jpg", @"E:\content.bin", ]);
	public static readonly ArgumentType		DIRPATH		= new (nameof(DIRPATH),		"A text string of the local directory path in its native format",		[@"C:\Folder", @"D:\Project\Content"]);
	public static readonly ArgumentType		PORT		= new (nameof(PORT),		"Port number",															["3800"]);
	public static readonly ArgumentType		PRIVATEKEY	= new (nameof(PRIVATEKEY),	"Hexadecimal text string of account private key",						["f5eb914b0cdf95fb3df9bcf7e3686cb16d351edf772e577dd6658f841f51b848"]);
	public static readonly ArgumentType		NAME		= new (nameof(NAME),		"An arbitrary single-line string without spaces",						["satoshi", "nakamoto", "hagardjuna"]);
	public static readonly ArgumentType		NA			= new (nameof(NA),			"A name of a network",													["rdn", "fair"]);
	public static readonly ArgumentType		NN			= new (nameof(NN),			"Full address of a network",											["fair.rdn"]);
	public static readonly ArgumentType		ST			= new (nameof(ST),			"Space-time in form of byte-years(BY), byte-days(BD), eth.",			["300bd", "500by"]);
	public static readonly ArgumentType		EC			= new (nameof(EC),			"Execution Cycles in form of integer number",							["1000", "10"]);
	public static readonly ArgumentType		TEXT		= new (nameof(TEXT),		"Arbitrary text, can be multi-line",									["\"Hello world!\"", "\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua\"", "\"Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur\""]);
	public static readonly ArgumentType		OPEARATION	= new (nameof(OPEARATION),	"MCV operation",														["{utility transfer from=user/12390-0 to=user/12345-6 e=100}"]);
	public static readonly ArgumentType		UID			= new (nameof(UID),			"User Id",																["123456-789"]);
	public static readonly ArgumentType		SNQ			= new (nameof(SNQ),			"URI-derived scheme-network-query address",								[new Snq(Iccp.Scheme, "fair", "author/123-234")]);
	public static readonly ArgumentType		STRING		= new (nameof(STRING),		"An arbitrary single-line string",										["sato shi", "na+ka+mo+to"]);
	public static readonly ArgumentType		URI			= new (nameof(URI),			"Fully-qualified URI address",											["http://fair.net", "iccp:fair/author"]);
	public static readonly ArgumentType		URL			= new (nameof(URL),			"Fully-qualified URL address",											["http://fair.net", "http://ultranet.org"]);
	public static readonly ArgumentType		ZONE		= new (nameof(ZONE),		"Zone name",															[Zone.Main.ToString(), Zone.Test.ToString()]);
	
	protected string						First => Args.Count > 0 ? Args[0].Name : throw new SyntaxException("No arguments provided");
	protected string						Second => Args.Count > 1 ? Args[1].Name : throw new SyntaxException("Second argument not provided");
	
	public const string						AORKeyword = "aor";
	public const string						ByKeyword = "by";
	public const string						BoostKeyword = "boost";
	
	public const string						IdKeyword = "id";
	public const string						NameKeyword = "name";
	public const string						AddressKeyword = "address";

	public Argument							NameArgument(ArgumentType type, string entity) => new (NameKeyword, type, $"Name of the {entity}");
	public Argument							AddressArgument(ArgumentType type, string entity) => new (AddressKeyword, type, $"Address of the {entity}");
	public Argument							IdArgument(string entity) => new (IdKeyword, EID, $"Id of the {entity}");
	public Argument							AddressOrId(ArgumentType type, string entity) => new(null, null, "OR", arguments:	[
																																	AddressArgument(type, entity),
																																	IdArgument(entity)
																																]);
	public Argument							NameOrId(ArgumentType type, string entity) => new(null, null, "OR", arguments:	[
																																NameArgument(type, entity),
																																IdArgument(entity)
																															]);
	public AutoId							Id => AutoId.Parse(GetString(IdKeyword));
	public string							Name => GetString(NameKeyword);
	public string							Address => GetString(AddressKeyword);
	public virtual string[]					TransactionKeywords => [AORKeyword, ByKeyword, BoostKeyword];

	protected NetCommand(List<Xon> args, Flow flow) : base(args, flow)
	{
	}

	protected void Run(Cli cli, CommandAction action)
	{
 		if(ConsoleAvailable)
		{
			var logview = new ConsoleLogView(false, false);

			logview.StartListening(Flow.Log);

			while(Flow.Active)
			{
				Console.Write($"{Flow.Name} >");

				try
				{
					var x = new Xon(Console.ReadLine());

					if(x.Nodes[0].Name == Keyword && (
														action.Names.Contains(x.Nodes[1].Name) 
													))
						throw new Exception("Not available");

					cli.Execute(x.Nodes, Flow);
				}
				catch(Exception ex)
				{
					Flow.Log.ReportError(this, "Error", ex);
				}
			}

			logview.StopListening();
		}
		else
			WaitHandle.WaitAny([Flow.Cancellation.WaitHandle]);
	}

	public TransactionApe Transact(McvApiClient api, IEnumerable<Operation> operations, string user, long boost, ActionOnResult aor)
	{
		var t = api.Call<TransactionApe>(new TransactApc
										 {
											Operations = operations,
											User = user,
											Boost = boost,
											ActionOnResult = aor
										 },
										 Flow);
		int n = 0;

		do 
		{
			t = api.Call<TransactionApe>(new OutgoingTransactionApc {Tag = t.Tag}, Flow);

			if(t.Status != TransactionStatus.FailedOrNotFound)
			{
				foreach(var i in t.Log.Skip(n))
				{
					foreach(var j in i.Text)
						Report(j);
				}
	
				n += t.Log.Length;
			}

			if(!NodeGlobals.NoWait)
				Thread.Sleep(1);
		}
		while(!(aor == ActionOnResult.RetryUntilConfirmed && t.Status == TransactionStatus.Confirmed || 
				aor == ActionOnResult.ExpectFailure && t.Status == TransactionStatus.FailedOrNotFound ||
				aor == ActionOnResult.CancelOnFailure && (t.Status == TransactionStatus.FailedOrNotFound || t.Status == TransactionStatus.Confirmed) ||
				aor == ActionOnResult.DoNotCare));

		if(t.Status == TransactionStatus.Confirmed)
			Flow.Log.Dump(t);

		return t;
	}

	public AccountAddress GetAccountAddress(string paramenter)
	{
		if(Has(paramenter))
			return AccountAddress.Parse(GetString(paramenter));
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	public AccountAddress GetAccountAddress(string paramenter, AccountAddress def)
	{
		if(Has(paramenter))
			return AccountAddress.Parse(GetString(paramenter));
		else
			return def;
	}

	protected E GetEnum<E>(string paramenter, E def) where E : struct
	{
		var p = One(paramenter);

		if(p != null)
			return Enum.Parse<E>(p.Get<string>(), true);
		else
			return def;
	}

	protected E GetEnum<E>(string paramenter) where E : struct
	{
		var p = One(paramenter);

		if(p != null)
			return Enum.Parse<E>(p.Get<string>(), true);
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	protected E GetEnum<E>(int index) where E : struct
	{
		try
		{
			return Enum.Parse<E>(Args[index].Name, true);
		}
		catch(Exception)
		{
			throw new SyntaxException($"Parameter at {index} position is not provided or incorrect");
		}
	}
}
