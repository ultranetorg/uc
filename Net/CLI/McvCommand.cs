using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Uccs.Net;

public abstract class McvCommand : NetCommand
{
	public Action							Transacted;
	protected McvCli						Cli;

	public static readonly ArgumentType		YEARS	= new ("YEARS",	@"Number of years, in [1..10] range",	["5"]);
	public static readonly ArgumentType		ET		= new ("ET",	@"Entity Type",							[McvTable.User, McvTable.Subnet]);
	public static readonly ArgumentType		EA		= new ("EA",	@"Entity address as {TableName}/{EID}",	[EntityAddress.Format(McvTable.User, new AutoId(1111, 22)), 
																											 EntityAddress.Format(McvTable.User, new AutoId(333, 444444)), 
																											 EntityAddress.Format(McvTable.User, new AutoId(1234567, 890))]);
	public static readonly ArgumentType		BOOL	= new ("BOOL",	@"Yes or No",							["yes, no"]);

	public static Argument					ByArgument(string description = "Name of the user") => new (ByKeyword, NAME, description);

	protected McvCommand(McvCli cli, List<Xon> args, Flow flow) : base(args, flow)
	{
		Cli = cli;
	}

	protected void ReportPreambule()
	{
	}

	protected void ReportNetwork()
	{
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

	public static ActionOnResult GetActionOnResult(IEnumerable<Xon> args)
	{
		var a = args.FirstOrDefault(i => i.Name == AORKeyword);

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

	protected long GetSpacetime(string paramenter, long def)
	{
		var p = One(paramenter);

		if(p != null)
			return User.ParseSpacetime(p.Get<string>());
		else
			return def;
	}

	protected ushort GetUInt16(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return ushort.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	protected long GetUInt16(string paramenter, ushort def)
	{
		var p = One(paramenter);

		if(p != null)
			return ushort.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			return def;
	}

}
