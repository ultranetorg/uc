using System.Reflection;
using Uccs.Net;
using Uccs.Nexus;

namespace Uccs.Nexus.CLI;

public class WalletCommand : NexusCommand
{
	public WalletCommand(NexusCli vault, List<Xon> args, Flow flow) : base(vault, args, flow)
	{
	}

	public WalletCommand() 
	{
	}

	public CommandAction Create_C()
	{
		const string keys = nameof(keys);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Used to create a new account and its wallet.";
		a.Arguments =	[
							new (NameKeyword, FILENAME, "An arbitrary name of a newly created wallet"),
							new (keys, INT, "Number of accounts to automatically create in the wallet", ArgumentFlag.Optional, @default: 0),
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

								var v = new Vault(Cli.NexusSettings.Zone, Cli.VaultSettings, Flow);
								var w = v.CreateWallet(Name, GetString("password"), GetInt(keys, (int)a.Arguments[1].Default));

								VaultApi(new ImportWalletApc {Name = GetString(NameKeyword), Raw = w.ToRaw()});

								foreach(var i in w.Keys.Index())
								{
									Report($"Key {i.Index}:");
									Report($"   Public - {i.Item.Public}");
									Report($"   Secret - {i.Item.Secret.Secret.ToHex()}");
								}

								return w;
							};
		return a;
	}

	public CommandAction ListWallets_LW()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Lists all existing wallets";
		a.Execute = () =>	{
								var r = VaultApi<WalletsApc.Wallet[]>(new WalletsApc {});

								Flow.Log.Dump(r, ["Name", "State"], [i => i.Name, i => i.Locked ? "Locked" : "Unlocked"]);

								return r;
							};
		return a;
	}

	public CommandAction ListKeys_LK()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Lists all accounts of the specified wallet";
		a.Arguments =	[
							new (NameKeyword, FILENAME, "Name of the wallet", ArgumentFlag.Optional),
						];
		a.Execute = () =>	{
								var r = VaultApi<WalletKeysApc.Key[]>(new WalletKeysApc {Name = GetString(NameKeyword, null)});

								Flow.Log.Dump(r, ["Name", "Public Key"], [i => i.Alias, i => i.Public]);

								return r;
							};
		return a;
	}

	public CommandAction Unlock_U()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Unlocks an existing wallet making it available for signing transactions";
		a.Arguments =	[
							new (NameKeyword, FILENAME, "Name of the wallet", ArgumentFlag.Optional),
							new ("password", PASSWORD, "Password of the wallet to unlock"),
						];

		a.Execute = () =>	{
								VaultApi(new UnlockWalletApc{Name = GetString(NameKeyword, null), 
														Password = GetString("password")});
								return null;
							};
		return a;
	}

	public CommandAction Lock_L()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Locks an existing wallet";
		a.Arguments =	[new (NameKeyword, FILENAME, "Name of the wallet", ArgumentFlag.Optional)];

		a.Execute = () =>	{
								VaultApi(new LockWalletApc {Name = GetString(NameKeyword, null)});
								return null;
							};
		return a;
	}

	public CommandAction AddKey_AK()
	{
		const string key = nameof(key);
		const string alias = nameof(alias);
		const string tag = nameof(tag);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Creates a new or import existing account to a wallet";
		a.Arguments =  [new (NameKeyword, FILENAME, "Name of a wallet to add the account to. Otherwise the default is used.", ArgumentFlag.Optional),
						new (alias, STRING, "Hint name of the key", ArgumentFlag.Optional),
						new (key, SECKEY, "Private key to import", ArgumentFlag.Optional)];

		a.Execute = () =>	{
								var aa = VaultApi<PublicKey>(new AddKeyToWalletApc {Wallet = GetString(NameKeyword, null), Key = GetBytes(key, null), Alias = GetString(alias, null), Tag = GetString(tag, null)});
								
								Report("Public Key : " + aa); 

								return aa;
							};
		return a;
	}

	public CommandAction Import_I()
	{
		var p = "path";

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Imports existing wallet using file store";
		a.Arguments =	[
							new (NameKeyword, FILENAME, "Name under which the wallet is stored", ArgumentFlag.Optional),
							new (p, FILEPATH, "Path to the source wallet file"),
						];

		a.Execute = () =>	{
								var	b = File.ReadAllBytes(GetString(p));

								VaultApi(new ImportWalletApc {Name = GetString(NameKeyword, null), Raw = b});
		
								return a;
							};
		return a;
	}
}
