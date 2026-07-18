using System.Reflection;

namespace Uccs.Net;

public abstract class McvCli : NetCli
{
	public McvNodeSettings	Settings;
	public McvNet			Net;
	public McvNode			Node;
	public McvApiClient		ApiClient;

	public McvCli()
	{
	}

	public McvCli(NexusSettings nexussettings, McvNodeSettings settings, McvApiClient api)
	{
		NexusSettings = nexussettings;
		Settings = settings;
		ApiClient = api;
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow);
	}

	public override void PostExecute(IEnumerable<Xon> args, Command command, object result, Flow flow)
	{
		var c = command as McvCommand;

		if(result is Operation || result is IEnumerable<Operation>)
		{
			Operation[] ops = result is Operation o ? [o] : [..result as IEnumerable<Operation>];

			//var u = c.Api<UserPpr>(new PpcApc {Request = new UserPpc(c.GetString(McvCommand.ByArg))});

			if(c.Has("estimate"))
			{
				var rp = c.Api<PretransactingPpr>(new EstimateOperationApc {Operations = ops, User = c.GetString(McvCommand.ByKeyword)});
				flow.Log.Dump(rp);
			}
			else
			{
				var t = c.Transact(ApiClient, ops, c.GetString(McvCommand.ByKeyword), c.GetLong(McvCommand.BoostKeyword, 0), McvCommand.GetActionOnResult(args));

				c.Transacted?.Invoke();
			}
		}
	}
}
