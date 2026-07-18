using System.Reflection;
using Uccs.Nexus;

namespace Uccs.Vault.CLI;

public class VaultCli : NetCli
{
	public VaultSettings			Settings;
	public IPasswordAsker			PasswordAsker = new ConsolePasswordAsker();
	public override VaultApiClient	Api => _Api ??= new VaultApiClient(Settings.Api.LocalSystemAddress(NexusSettings.Zone, Net.Api.Vault));
	VaultApiClient					_Api;

	public VaultCli()
	{
		Boot = new NetBoot(ExeDirectory);
		NexusSettings = new NexusSettings(Boot.Zone, Boot.Profile);
		Settings = new VaultSettings(NexusSettings);

		Execute(Boot.Profile, Boot.Commnand);
	}

	public VaultCli(NexusSettings nexussettings, VaultSettings settings)
	{
		NexusSettings = nexussettings;
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
