using System.Reflection;
using Uccs.Nexus;

namespace Uccs.Vault.CLI;

public class VaultCommand : NetCommand
{
	public VaultCli	Cli;

	public VaultCommand(VaultCli vault, List<Xon> args, Flow flow) : base(args, flow)
	{
		Cli = vault;
	}

	public void Api(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;
			
		if(call is IVaultApc u)
		{
			if(Cli.Vault != null)
				u.Execute(Cli.Vault, null, null, Flow);
			else
				Cli.Api.Send(call, Flow);

			return;
		}

		throw new Exception();
	}

	public R Api<R>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		if(call is IVaultApc u)	
			if(Cli.Vault != null)
				return (R)u.Execute(Cli.Vault, null, null, Flow);
			else
				return Cli.Api.Call<R>(call, Flow);

		throw new Exception();
	}

//	public CommandAction Run()
//	{
// 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
//
//		a.Name = "r";
//		a.Description = "Runs a new instance with command-line interface";
//		a.Arguments =	[
//							new ("profile", DIRPATH, "Path to local profile directory", ArgumentFlag.Optional),
//						];
//
//		a.Execute = () =>	{
//								Run(Cli, a);
//
//								return null;
//							};
//		
//		return a;
//	}

}
