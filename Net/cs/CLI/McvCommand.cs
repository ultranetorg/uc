using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Uccs.Net;

public abstract class McvCommand : NetCommand
{
	public const string		AwaitArg = "await";
	public const string		SignerArg = "signer";
	public Action			Transacted;
	protected McvCli		Cli;

	public readonly ArgumentType EID	= new ArgumentType("EID",	@"Entity Id",	[@"1111-22", @"123456-789"]);

	static McvCommand()
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

		return Cli.ApiClient.Request<Rp>(call, Flow);
	}

	public Rp Ppc<Rp>(Ppc<Rp> call) where Rp : PeerResponse
	{
		var rp = Api<Rp>(new PpcApc {Request = call});
 
 		if(rp.Error != null)
 			throw rp.Error;
 
		return rp;
	}

	public OutgoingTransaction Transact(IEnumerable<Operation> operations, AccountAddress signer, TransactionStatus await)
	{
		var t =  Cli.ApiClient.Request<OutgoingTransaction>(new TransactApc {Operations = operations,
																			 Signer = signer,
																			 Await = await}, Flow);
		
		int n = 0;

		do 
		{
			t = Cli.ApiClient.Request<OutgoingTransaction>(new OutgoingTransactionApc {Tag = t.Tag}, Flow);

			foreach(var i in t.Log.Skip(n))
			{
				foreach(var j in i.Text)
					Report(j);
			}

			n += t.Log.Length;

			Thread.Sleep(10);
		}
		while(t.Status != await);

		if(t.Status == TransactionStatus.Confirmed)
			Dump(t);

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


	public static TransactionStatus GetAwaitStage(IEnumerable<Xon> args)
	{
		var a = args.FirstOrDefault(i => i.Name == AwaitArg);

		if(a != null)
		{
			return Enum.GetValues<TransactionStatus>().First(i => i.ToString().ToLower() == a.Get<string>());
		}
		else
			return TransactionStatus.Placed;
	}

	protected EntityId GetEntityId(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return EntityId.Parse(p.Get<string>());
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	protected EntityId GetEntityId(string paramenter, EntityId @default)
	{
		var p = One(paramenter);

		if(p != null)
			return EntityId.Parse(p.Get<string>());
		else
			return @default;
	}

	protected long GetMoney(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	protected long GetMoney(string paramenter, long def)
	{
		var p = One(paramenter);

		if(p != null)
			return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			return def;
	}
}
