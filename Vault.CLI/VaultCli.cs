using System.Reflection;
using Uccs.Nexus;

namespace Uccs.Vault.CLI;

public class VaultCli : Cli
{
	public VaultSettings		Settings;
	public IPasswordAsker		PasswordAsker = new ConsolePasswordAsker();
	public NetBoot				Boot;
	public VaultApiClient		Api => _Vault ??= new VaultApiClient(Settings.Api.LocalSystemAddress(Boot.Zone, Net.Api.Vault));
	VaultApiClient				_Vault;

	public Nexus.Vault			Vault;

	static VaultCli()
	{
	}

	public VaultCli()
	{
		Boot = new NetBoot(ExeDirectory);
		var ns = new NexusSettings(Boot.Zone, Boot.Profile);
		Settings = new VaultSettings(ns);

		Execute(Boot.Profile, Boot.Commnand);
	}

	public VaultCli(NetBoot boot, VaultSettings settings)
	{
		Boot = boot;
		Settings = settings;
	}

	public static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new VaultCli();
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow);
	}
}
