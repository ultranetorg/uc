using System.Reflection;

namespace Uccs.Net;

public class McvCli : Cli
{
	public NexusSettings	NexusSettings;
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
		var t = commnad.First().Name;

		var args = commnad.Skip(1).ToList();

		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		return ct?.GetConstructor([typeof(McvCli), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as McvCommand;
	}

	public virtual object Execute(NetBoot boot, Flow flow)
	{
		return Execute(boot.Commnand.Nodes, flow);
	}

	public override void PostExecute(IEnumerable<Xon> args, Command command, object result, Flow flow)
	{
		var c = command as McvCommand;

		if(result is Operation o)
		{
			if(c.Has("estimate"))
			{
				var rp = c.Api<AllocateTransactionPpr>(new EstimateOperationApc {Operations = [o], By = c.GetAccountAddress(McvCommand.SignerArg)});
				flow.Log.Dump(rp);
			}
			else
			{
				var t = c.Transact([o], c.GetAccountAddress(McvCommand.SignerArg), McvCommand.GetActionOnResult(args));

				c.Transacted?.Invoke();
			}
		}
		else if(result is IEnumerable<Operation> ooo)
		{
			if(c.Has("estimate"))
			{
				var rp = c.Api<AllocateTransactionPpr>(new EstimateOperationApc {Operations = ooo, By = c.GetAccountAddress(McvCommand.SignerArg)});
				flow.Log.Dump(rp);
			}
			else
			{
				var t = c.Transact(ooo, c.GetAccountAddress(McvCommand.SignerArg), McvCommand.GetActionOnResult(args));

				c.Transacted?.Invoke();
			}
		}
	}
}
