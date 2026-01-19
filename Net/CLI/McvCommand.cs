using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Uccs.Net;

public abstract class McvCommand : NetCommand
{
	public const string					AORArg = "aor";
	public const string					ByArg = "by";
	//public const string					SignerArg = "signer";
	public Action						Transacted;
	protected McvCli					Cli;

	public static readonly ArgumentType YEARS	= new ("YEARS",	@"Number of years",	[@"5"]);
	public static readonly ArgumentType	ET		= new ("ET",	@"Entity Type",		[@"Account", @"Domain"]);
	public static readonly ArgumentType	EID		= new ("EID",	@"Entity Id",		[@"1111-22", @"123456-789", @"22222-333"]);
	public static readonly ArgumentType	EA		= new ("EA",	@"Entity address",	[@"Account/1111-22", @"Account/123456-789", @"Account/22222-333"]);

	protected Argument					ByArgument(string description = "User name") => new (ByArg, NAME, description);

	protected McvCommand(McvCli cli, List<Xon> args, Flow flow) : base(args, flow)
	{
		Cli = cli;
	}


	protected void ReportPreambule()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

		Flow.Log.Report(fvi.ProductName);
		Flow.Log.Report(fvi.FileVersion);
	}

	protected void ReportNetwork()
	{
		Flow.Log.Report($"Current Net    : {Cli.Net}");
	}

	public void Api(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		Cli.ApiClient.Send(call, Flow);
	}

	public Rp Api<Rp>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		return Cli.ApiClient.Call<Rp>(call, Flow);
	}

	public Rp Ppc<Rp>(Ppc<Rp> call) where Rp : Result
	{
		var rp = Api<Rp>(new PpcApc {Request = call});
 
 		//if(rp.Error != null)
 		//	throw rp.Error;
 
		return rp;
	}

	public TransactionApe Transact(IEnumerable<Operation> operations, string user, ActionOnResult aor)
	{
		var t = Cli.ApiClient.Call<TransactionApe>(	new TransactApc
													{
														Operations = operations,
														User = user,
														Application = Assembly.GetEntryAssembly().Location,
														ActionOnResult = aor
													},
													Flow);
		int n = 0;

		do 
		{
			t = Cli.ApiClient.Call<TransactionApe>(new OutgoingTransactionApc {Tag = t.Tag}, Flow);

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

	//protected AccountKey GetPrivate(string walletarg)
	//{
	//	string p = null;
	//	
	//	var a = new ConsolePasswordAsker();
	//	a.Ask(GetString(walletarg));
	//	p = a.Password; 
	//
	//	return Sun.Vault.Unlock(AccountAddress.Parse(GetString(walletarg)), p);
	//}


	public static ActionOnResult GetActionOnResult(IEnumerable<Xon> args)
	{
		var a = args.FirstOrDefault(i => i.Name == AORArg);

		if(a != null)
		{
			return Enum.GetValues<ActionOnResult>().First(i => i.ToString().ToLower() == a.Get<string>());
		}
		else
			return ActionOnResult.RetryUntilConfirmed;
	}

	protected AutoId GetAutoId(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return AutoId.Parse(p.Get<string>());
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	protected AutoId GetEntityId(string paramenter, AutoId @default)
	{
		var p = One(paramenter);

		if(p != null)
			return AutoId.Parse(p.Get<string>());
		else
			return @default;
	}

	protected long GetBD(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return User.ParseSpacetime(p.Get<string>());
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	protected long GetBD(string paramenter, long def)
	{
		var p = One(paramenter);

		if(p != null)
			return User.ParseSpacetime(p.Get<string>());
		else
			return def;
	}

	protected long GetEC(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	protected long GetEC(string paramenter, long def)
	{
		var p = One(paramenter);

		if(p != null)
			return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			return def;
	}

}
