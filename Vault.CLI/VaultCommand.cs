using System.Reflection;
using Uccs;
using Uccs.Vault.CLI;

public class VaultCommand : NetCommand
{
	new protected VaultCli Cli => base.Cli as VaultCli;

	public VaultCommand(VaultCli vault, List<Xon> args, Flow flow) : base(args, flow)
	{
		base.Cli = vault;
	}

	public VaultCommand()
	{
	}

}
