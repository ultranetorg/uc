using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.CLI;

public abstract class NexusCommand : NetCommand
{
	new public NexusCli	Cli => base.Cli as NexusCli;

	public NexusCommand(NexusCli uos, List<Xon> args, Flow flow) : base(args, flow)
	{
		base.Cli = uos;

		Flow.Log?.TypesForExpanding.AddRange(  [typeof(IEnumerable<Dependency>), 
												typeof(IEnumerable<AnalyzerReport>), 
												typeof(Resource), 
												typeof(PackageManifest)]);
	}

	public NexusCommand()
	{
	}

	public override void Api(Apc call)
	{
		ApplyTimeouts(call);
			
		if(call is INexusApc u)
		{
			if(Cli.Nexus != null)
				u.Execute(Cli.Nexus, null, null, Flow);
			else
				Cli.Api.Send(call, Flow);

			return;
		}

		throw new Exception();
	}

	public override R Api<R>(Apc call)
	{
		ApplyTimeouts(call);

		if(call is INexusApc u)	
			if(Cli.Nexus != null)
				return (R)u.Execute(Cli.Nexus, null, null, Flow);
			else
				return Cli.Api.Call<R>(call, Flow);

		throw new Exception();
	}

	public void VaultApi(Apc call)
	{
		ApplyTimeouts(call);

		if(call is IVaultApc v)
			v.Execute(Cli.Nexus.Vault, null, null, Flow);
		else
			Cli.VaultApi.Send(call, Flow);
	}

	public R VaultApi<R>(Apc call)
	{
		ApplyTimeouts(call);

		if(call is IVaultApc v)	
			return (R)v.Execute(Cli.Nexus.Vault, null, null, Flow);
		else
			return Cli.VaultApi.Call<R>(call, Flow);

		throw new Exception();
	}

	protected Ura GetResourceAddress(string paramenter, Ura def)
	{
		if(Has(paramenter))
			return Ura.Parse(GetString(paramenter));
		else
			return def;
	}

	protected Ura GetResourceAddress(string paramenter)
	{
		if(Has(paramenter))
			return Ura.Parse(GetString(paramenter));
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}
}
