using Uccs.Nexus;

namespace Uccs.Vault.CLI;

public class VaultCli : Cli
{
	public VaultSettings		Settings;
	public Cryptography			Cryptography;
	public IPasswordAsker		PasswordAsker = new ConsolePasswordAsker();

	public Nexus.Vault			Vault;
	public NetBoot				Boot;

	VaultApiClient				_Vault;
	public VaultApiClient		VaultApi => _Vault ??= new VaultApiClient(Settings.Api.LocalSystemAddress(Boot.Zone, Api.Vault));

	static VaultCli()
	{
	}

	public VaultCli()
	{
		Boot = new NetBoot(ExeDirectory);
		Settings = new VaultSettings(Boot.Profile);

		Execute(Boot);
	}

	public VaultCli(Nexus.Vault vault)
	{
		Vault = vault;
	}

	public static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new VaultCli();
	}
}
