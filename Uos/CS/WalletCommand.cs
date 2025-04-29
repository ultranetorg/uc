using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos;

public class WalletCommand : UosCommand
{
	public WalletCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Used to create a new account and its wallet.",
						Syntax = $"{Keyword} {a.NamesSyntax} [password={PASSWORD}]",

						Arguments =	[
										new ("password", "A password that is used to encrypt the newly created wallet")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} password={PASSWORD.Example}")
									]};

		a.Execute = () =>	{
								string p = GetString("password", null);

								if(p == null)
								{
									Uos.PasswordAsker.Create(Vault.PasswordWarning);
									p = Uos.PasswordAsker.Password;
								}

								var w = Uos.Vault.CreateWallet(p);

								Report("Public Address - " + w.Accounts.First().Address); 
								Report("Private Key    - " + w.Accounts.First().Key.PrivateKey.ToHex());

								Api(new AddWalletApc {Raw = w.Raw});

								return w;
							};
		return a;
	}

	public CommandAction List()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Help = new() {Description = "Lists all existing wallets",
						Syntax = $"{Keyword} {a.NamesSyntax}",

						Arguments =	[],

						Examples =	[
										new (null, $"{Keyword} {a.Name}")
									]};

		a.Execute = () =>	{
								var r = Api<WalletsApc.WalletApe[]>(new WalletsApc {});

								Flow.Log.Dump(r, ["Name", "State", "Accounts"], [i => i.Name, i => i.Locked ? "Locked" : "Unlocked", i => string.Join(", ", (object[])i.Accounts)]);

								return r;
							};
		return a;
	}

	public CommandAction Unlock()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "u";

		a.Help = new() {Description = "Unlocks an existing wallet making it available for signing transactions",
						Syntax = $"{Keyword} {a.NamesSyntax} password={PASSWORD} [name={NAME}]",

						Arguments =	[
										new ("password", "A password of a wallet to be unlocked"),
										new ("name", "Name of wallet")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} name={NAME.Example} password={PASSWORD.Example}")
									]};

		a.Execute = () =>	{
								Api(new UnlockWalletApc{Name = GetString("name", null), 
														Password = GetString("password")});
								return null;
							};
		return a;
	}

	public CommandAction Lock()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "l";

		a.Help = new() {Description = "Locks an existing wallet",
						Syntax = $"{Keyword} {a.NamesSyntax} {AA} [name={NAME}]",

						Arguments =	[new ("name", "Name of wallet")],
						Examples =	[new (null, $"{Keyword} {a.Name} {AA.Example} password={PASSWORD.Example}")]};

		a.Execute = () =>	{
								Api(new LockWalletApc {Name = GetString("name", null)});
								return null;
							};
		return a;
	}

	public CommandAction AddAccount()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "aa";
		a.Help = new() {Description = "Cerates a new or import existing account to a wallet",
						Syntax = $"{Keyword} {a.NamesSyntax} [name={NAME}] [key={PRIVATEKEY}]",

						Arguments =	[new ("name", "Name of wallet"),
									 new ("key", "Private key of account to import")],

						Examples =	[new (null, $"{Keyword} {a.Name} name={NAME.Example} key={PRIVATEKEY.Example}")]};

		a.Execute = () =>	{
								var pk = Api<byte[]>(new AddAccountToWalletApc {Name = GetString("name", null), Key = GetBytes("key", false) });
								
								var k = new AccountKey(pk);

								Report("Public Address - " + k); 
								Report("Private Key    - " + k.PrivateKey.ToHex());

								return k;
							};
		return a;
	}

	public CommandAction Import()
	{
		var p = "path";

		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "i";

		a.Help = new() {Description = "Imports existing wallet",
						Syntax = $"{Keyword} {a.NamesSyntax} {p}={FILEPATH}",

						Arguments =	[
										new (p, "A path to a source wallet file"),
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {p}={DIRPATH}\\wallet.{Vault.EncryptedWalletExtention}")
									]};

		a.Execute = () =>	{
								var	b = File.ReadAllBytes(GetString(p));

								Api(new AddWalletApc {Raw = b});
		
								return a;
							};
		return a;
	}
}
