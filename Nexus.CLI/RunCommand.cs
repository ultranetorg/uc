using System.Reflection;
using Uccs.Net;

namespace Uccs.Nexus.CLI;

public class RunCommand : NexusCommand
{

	public RunCommand(NexusCli uos, List<Xon> args, Flow flow) : base(uos, args, flow)
	{
	}
	public RunCommand()
	{
	}

	public CommandAction Default()
	{
 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Runs a new instance with command-line interface";
		a.Arguments =	[
							new ("profile", DIRPATH, "Path to local profile directory", ArgumentFlag.Optional),
							new ("zone", ZONE, "Zone name", ArgumentFlag.Optional),
						];

		a.Execute = () =>	{
								var b = Cli.Boot;

								var ns = new NexusSettings(b.Zone, b.Profile);
								var vs = new VaultSettings(ns);

								Cli.Nexus = new Nexus(b, ns, vs, new Flow(nameof(Nexus), new Log()){WorkDirectory = b.Profile});
								Cli.Nexus.Vault.AuthenticationRequested += OnAuth;
								Cli.Nexus.RunRdn(null, new RealClock());

								Cli.InteractOrWait(b.Profile, this, a, Cli.Nexus.Flow);

								Cli.Nexus.Vault.AuthenticationRequested -= OnAuth;
								Cli.Nexus.Stop();

								return null;
							};

		return a;
	}

	AuthenticationChoice OnAuth(string application, byte[] logo, string net, string user, AccountAddress account)
	{
		var a = new Authentication
				{
					Application = application,
					//Logo		= logo,
					Net			= net,
					User		= user,
					Session		= Cryptography.RandomBytes(32), 
				};


		var ac = Cli.Nexus.Vault.Find(account)
				 ??
				 throw new VaultException(VaultError.NotFound);
						
		ac.PendingAuthentications.Add(a);

		return new AuthenticationChoice {Waiting = true};
	}
}