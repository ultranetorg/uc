using System.Reflection;
using Uccs;
using Uccs.Nexus;
using Uccs.Vault.CLI;

public abstract class VaultCommand : NetCommand
{
	public VaultCli	Cli;

	public VaultCommand(VaultCli vault, List<Xon> args, Flow flow) : base(args, flow)
	{
		Cli = vault;
	}

	public VaultCommand()
	{
	}

	public void Api(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;
			
		Cli.Api.Send(call, Flow);
	}

	public R Api<R>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		return Cli.Api.Call<R>(call, Flow);
	}
}
