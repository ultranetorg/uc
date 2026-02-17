using System.Reflection;

namespace Uccs.Net;

public abstract class NetCommand : Command
{

	public readonly ArgumentType AA			= new ("AA",		@"Account address, in hexadecimal form prepended with '0x'",			["0x0000A5A0591B2BF5085C0DDA2C39C5E478300C68", "0x0001D9867473B229B5F13BF594E0DF9D4F61F8ED", "0x0002C6C15D653236A811A1115A6F08F0BC54D2B7", "0x0003924B07CE25C18667899EFFF15CBF5C75C1EE"]);
	public readonly ArgumentType COMMAND	= new ("COMMAND",	@"Arbitrary command",													["{...}"]);
	public readonly ArgumentType IP			= new ("IP",		@"IP Address",															["12.34.56.78"]);
	public readonly ArgumentType INT		= new ("INT",		@"Positive integer number",												["324552"]);
	public readonly ArgumentType HEX		= new ("HEX",		@"Array od bytes in form of hexadecimal string",						["0105BCE1C336874FBEBE40D2510EC035D0251FE855399EAD76E22BD18E2EBC6E37"]);
	public readonly ArgumentType HOST		= new ("HOST",		@"Host name",															["1.2.3.4"]);
	public readonly ArgumentType PASSWORD	= new ("PASSWORD",	@"Text string of any no. chars and longer than 1 char",					["MyStrongSecret!@#$%^&*()"]);
	public readonly ArgumentType PATH		= new ("PATH",		@"A path to local file or directory in native format",					[@"C:\User\readme.txt", @"C:\User\Admin", @"D:\Documents"]);
	public readonly ArgumentType FILEPATH	= new ("FILEPATH",	@"A text string of the local file path in its native format",			[@"C:\User\file", @"D:\image.jpg", @"E:\content.bin", ]);
	public readonly ArgumentType DIRPATH	= new ("DIRPATH",	@"A text string of the local directory path in its native format",		[@"C:\Folder"]);
	public readonly ArgumentType PORT		= new ("PORT",		@"Port number",															["3800"]);
	public readonly ArgumentType PRIVATEKEY	= new ("PRIVATEKEY",@"Hexadecimal text string of account private key",						["f5eb914b0cdf95fb3df9bcf7e3686cb16d351edf772e577dd6658f841f51b848"]);
	public readonly ArgumentType NAME		= new ("NAME",		@"An arbitrary single-line string without spaces",						["one", "second"]);
	public readonly ArgumentType NET		= new ("NET",		@"A name of a network",													["rdn"]);
	public readonly ArgumentType ST			= new ("ST",		@"Space-time in form of byte-years(BY), byte-days(BY), eth.",			["300bd", "500by"]);
	public readonly ArgumentType EC			= new ("EC",		@"Execution Cycles in form of integer number",							["1000"]);
	public readonly ArgumentType TEXT		= new ("TEXT",		@"Arbitrary text, can be multi-line",									["Hello world!"]);
	public readonly ArgumentType UID		= new ("UID",		@"User Id",																["123456-789"]);
	public readonly ArgumentType UNEL		= new ("UNEL",		@"Universal network-entity address",									["ccp:fair/author/123-234"]);
	public readonly ArgumentType URL		= new ("URL",		@"Fully-qualified URL address",											["http://fair.net", "http://ultranet.org"]);
	public readonly ArgumentType ZONE		= new ("ZONE",		@"Zone name",															[Zone.Main.ToString(), Zone.Test.ToString()]);
		
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

	public TransactionApe Transact(McvApiClient api, IEnumerable<Operation> operations, string application, string user, ActionOnResult aor)
	{
		var t = api.Call<TransactionApe>(	new TransactApc
											{
												Operations = operations,
												User = user,
												Application = application,
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
