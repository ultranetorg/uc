using System.Reflection;
using Uccs.Nexus;

namespace Uccs.Vault.CLI;

public class WalletCommand : VaultCommand
{
	public WalletCommand(VaultCli vault, List<Xon> args, Flow flow) : base(vault, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Used to create a new account and its wallet.";
		a.Arguments =	[
							new ("name", NAME, "An arbitrary name of a newly created wallet"),
							new ("accounts", INT, "Count of account to create in the wallet"),
							new ("password", PASSWORD, "A password that is used to encrypt a newly created wallet", ArgumentFlag.Optional)
						];

		a.Execute = () =>	{
								//string p = GetString("password", null);
								//
								//if(p == null)
								//{
								//	Cli.PasswordAsker.Create(Nexus.Vault.PasswordWarning);
								//	p = Cli.PasswordAsker.Password;
								//}

								var v = Cli.Vault ?? new Nexus.Vault(Cli.Boot.Zone, Cli.Settings, Flow);
								var w = v.CreateWallet(GetString("name"), GetString("password"), GetInt("accounts", 1));

								Api(new AddWalletApc {Name = GetString("name"), Raw = w.ToRaw()});

								foreach(var i in w.Accounts.Index())
								{
									Report($"Account {i.Index}:");
									Report($"   Public Address - {i.Item.Address}");
									Report($"   Private Key    - {i.Item.Key.Secret.ToHex()}");
								}

								return w;
							};
		return a;
	}

	public CommandAction List()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "lw";
		a.Description = "Lists all existing wallets";

		a.Execute = () =>	{
								var r = Api<WalletsApc.Wallet[]>(new WalletsApc {});

								Flow.Log.Dump(r, ["Name", "State"], [i => i.Name, i => i.Locked ? "Locked" : "Unlocked"]);

								return r;
							};
		return a;
	}

	public CommandAction Unlock()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "u";

		a.Description = "Unlocks an existing wallet making it available for signing transactions";
		a.Arguments =	[
							new ("password", PASSWORD, "A password of a wallet to be unlocked"),
							new ("name", NAME, "Name of wallet", ArgumentFlag.Optional)
						];

		a.Execute = () =>	{
								Api(new UnlockWalletApc{Name = GetString("name", null), 
														Password = GetString("password")});
								return null;
							};
		return a;
	}

	public CommandAction Lock()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "l";

		a.Description = "Locks an existing wallet";
		a.Arguments =	[new ("name", NAME, "Name of wallet", ArgumentFlag.Optional)];

		a.Execute = () =>	{
								Api(new LockWalletApc {Name = GetString("name", null)});
								return null;
							};
		return a;
	}

	public CommandAction AddAccount()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "aa";
		a.Description = "Creates a new or import existing account to a wallet";
		a.Arguments =  [new ("wallet", NAME, "A name of a wallet to add the account to", ArgumentFlag.Optional),
						new ("name", NAME, "A name of account", ArgumentFlag.Optional),
						new ("key", PRIVATEKEY, "Private key of account to import")];

		a.Execute = () =>	{
								var pk = Api<byte[]>(new AddAccountToWalletApc {Wallet = GetString("wallet", null), Key = GetBytes("key", null), Name = GetString("name", null), Tag = GetString("tag", null)});
								
								var k = new AccountKey(pk);

								Report("Public Address - " + k); 
								Report("Private Key    - " + k.Secret.ToHex());

								return k;
							};
		return a;
	}

	public CommandAction Import()
	{
		var p = "path";

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "i";
		a.Description = "Imports existing wallet using file store";
		a.Arguments =	[
							new ("name", NAME, "Name of wallet", ArgumentFlag.Optional),
							new (p, FILEPATH, "A path to a source wallet file"),
						];

		a.Execute = () =>	{
								var	b = File.ReadAllBytes(GetString(p));

								Api(new AddWalletApc {Name = GetString("name", null), Raw = b});
		
								return a;
							};
		return a;
	}
}
