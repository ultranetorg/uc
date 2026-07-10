using System.Reflection;

namespace Uccs.Net;

public abstract class NetCommand : Command
{
	public readonly ArgumentType	AA			= new ("AA",		@"Account address, in Bech32 form",										["plsar7tfrq83mvfaw0m6u0l9jmsxhkjvk5zzanaqrhavfygl5w2sq3cyyk9btes", "gya27t2jsxdfa3gdywck9cdtwe6lhc59qwwrq2jdq6plz3k9hhjsqx0yuc6btes", "uh39t57ujhn4skcdhkg24mavdp4jsghtvv0uxeg2596et7h6vmnsqrntezqbtes", "vlrvxjhwz96gckyk9etxft3980agmy8s38u45gscp93ajk8rje5qqkgeww3btes"]);
	public readonly ArgumentType	COMMAND		= new ("COMMAND",	@"Arbitrary command",													["{...}"]);
	public readonly ArgumentType	IP			= new ("IP",		@"IP Address",															["12.34.56.78"]);
	public readonly ArgumentType	INT			= new ("INT",		@"Positive integer number",												["324552", "1000"]);
	public readonly ArgumentType	HEX			= new ("HEX",		@"Array od bytes in form of hexadecimal string",						["0105BCE1C336874FBEBE40D2510EC035D0251FE855399EAD76E22BD18E2EBC6E37"]);
	public readonly ArgumentType	HOST		= new ("HOST",		@"Host name",															["1.2.3.4"]);
	public readonly ArgumentType	PASSWORD	= new ("PASSWORD",	@"Text string of any no. chars and longer than 1 char",					["MyStrongSecret!@#$%^&*()"]);
	public readonly ArgumentType	PATH		= new ("PATH",		@"A path to local file or directory in native format",					[@"C:\User\readme.txt", @"C:\User\Admin", @"D:\Documents"]);
	public readonly ArgumentType	FILEPATH	= new ("FILEPATH",	@"A text string of the local file path in its native format",			[@"C:\User\file", @"D:\image.jpg", @"E:\content.bin", ]);
	public readonly ArgumentType	DIRPATH		= new ("DIRPATH",	@"A text string of the local directory path in its native format",		[@"C:\Folder"]);
	public readonly ArgumentType	PORT		= new ("PORT",		@"Port number",															["3800"]);
	public readonly ArgumentType	PRIVATEKEY	= new ("PRIVATEKEY",@"Hexadecimal text string of account private key",						["f5eb914b0cdf95fb3df9bcf7e3686cb16d351edf772e577dd6658f841f51b848"]);
	public readonly ArgumentType	NAME		= new ("NAME",		@"An arbitrary single-line string without spaces",						["one", "second"]);
	public readonly ArgumentType	NET			= new ("NET",		@"A name of a network",													["rdn"]);
	public readonly ArgumentType	ST			= new ("ST",		@"Space-time in form of byte-years(BY), byte-days(BY), eth.",			["300bd", "500by"]);
	public readonly ArgumentType	EC			= new ("EC",		@"Execution Cycles in form of integer number",							["1000", "10"]);
	public readonly ArgumentType	TEXT		= new ("TEXT",		@"Arbitrary text, can be multi-line",									["\"Hello world!\"", "\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua\"", "\"Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur\""]);
	public readonly ArgumentType	UID			= new ("UID",		@"User Id",																["123456-789"]);
	public readonly ArgumentType	SNP			= new ("SNP",		@"Universal scheme-network-path address",								["iccp:fair/author/123-234"]);
	public readonly ArgumentType	URI			= new ("URL",		@"Fully-qualified URI address",											["http://fair.net", "iccp:fair/author"]);
	public readonly ArgumentType	URL			= new ("URL",		@"Fully-qualified URL address",											["http://fair.net", "http://ultranet.org"]);
	public readonly ArgumentType	ZONE		= new ("ZONE",		@"Zone name",															[Zone.Main.ToString(), Zone.Test.ToString()]);
	
	protected string				First => Args.Count > 0 ? Args[0].Name : throw new SyntaxException("No arguments provided");
	
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

	public AccountAddress GetAccountAddress(string paramenter, bool mandatory = true)
	{
		if(Has(paramenter))
			return AccountAddress.Parse(GetString(paramenter));
		else
			if(mandatory)
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
			else
				return null;
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
