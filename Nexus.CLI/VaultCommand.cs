using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.CLI;

public class VaultCommand : NexusCommand
{
	public VaultCommand(NexusCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	public VaultCommand()
	{
	}

	public CommandAction RequestAuthentication_RA()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string user = nameof(user);
		const string net = nameof(net);
		const string application = nameof(application);
		const string account = nameof(account);

		a.Description = "Authenticates the specified user to transact in the specified network from the specified application";
		a.Arguments =	[
							new (account,		AA, "Address of the account which are authorized to sign transactions"),
							new (user,			NAME, "Name of user in the specified network on whose behalf transactions are to be sent"),
							new (net,			NA, "Address of the network to where transactions are to be sent"),
							new (application,	STRING, "Identifier of the application"),
						];

		a.Execute = () =>	{

								var ar = Api<AuthenticationResult>(	new AuthenticateApc
																	{
																		User = GetString(user),
																		Net = GetString(net),
																		Application = GetString(application),
																		Account  = GetAccountAddress(account, null)
																	});
								return ar;
							};
		return a;
	}

	public CommandAction ConfirmAuthentication_CA()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string account = nameof(account);
		const string session = nameof(session);
		const string trust = nameof(trust);

		a.Description = "Confirms pending authentication";
		a.Arguments =	[
							new (account, AA, "Name of the account to confirm authentication for"),
							new (session, HEX, "Session Id"),
							new (trust, TRUST, "Trust level"),
						];

		a.Execute = () =>	{
								if(Cli.Nexus.Vault == null)
									throw new VaultException(VaultError.NotPermittedOutsideVaultApplication);

								lock(Cli.Nexus.Vault)
								{
									var ac = Cli.Nexus.Vault.Find(GetString(account))
											 ??
											 throw new VaultException(VaultError.NotFound);
	
									var a =  ac.PendingAuthentications.Find(i => Bytes.Equal(i.Session, GetString(session).FromHex()))
											 ??
											 throw new VaultException(VaultError.NotFound);
	
									ac.AddAuthentication(a.Application, a.Net, a.User, null, GetEnum<Trust>(trust));
			
									ac.PendingAuthentications.Remove(a);
								}

								return null;
							};
		return a;
	}

	public CommandAction PendingAuthentications_LPA()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string account = nameof(account);

		a.Description = "Lists pending authentications for the specified account";
		a.Arguments =	[
							new (account, AA, "Name of the account to get pending authentications from"),
						];

		a.Execute = () =>	{
								if(Cli.Nexus.Vault == null)
									throw new VaultException(VaultError.NotPermittedOutsideVaultApplication);

								lock(Cli.Nexus.Vault)
								{
									var ac = Cli.Nexus.Vault.Find(GetAccountAddress(account))
											 ??
											 throw new VaultException(VaultError.NotFound);

									Flow.Log.Dump(ac.PendingAuthentications, ["Application", "Net", "User", "Session"], [i => i.Application, i => i.Net, i => i.User, i => i.Session.ToHex()]);	
								}

								return null;
							};
		return a;
	}

}
