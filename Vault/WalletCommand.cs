using System.Reflection;

namespace Uccs.Vault;

public class WalletCommand : VaultCommand
{
	public WalletCommand(Vault vault, List<Xon> args, Flow flow) : base(vault, args, flow)
	{
	}

	public void Api(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;
			
		if(call is IVaultApc u)
		{
			u.Execute(Vault, null, null, Flow);
			return;
		}

		throw new Exception();
	}

	public Rp Api<Rp>(Apc call)
	{
		if(Has("apitimeout"))
			call.Timeout = GetInt("apitimeout") * 1000;

		if(call is IVaultApc u)	
			return (Rp)u.Execute(Vault, null, null, Flow);

		throw new Exception();
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Used to create a new account and its wallet.",
						Arguments =	[
										new ("password", PASSWORD, "A password that is used to encrypt the newly created wallet", Flag.Optional)
									]};

		a.Execute = () =>	{
								string p = GetString("password", null);

								if(p == null)
								{
									Vault.PasswordAsker.Create(Vault.PasswordWarning);
									p = Vault.PasswordAsker.Password;
								}

								var w = Vault.CreateWallet(p);

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
		a.Help = new() {Description = "Lists all existing wallets"};

		a.Execute = () =>	{
								var r = Api<WalletsApc.Wallet[]>(new WalletsApc {});

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
						Arguments =	[
										new ("password", PASSWORD, "A password of a wallet to be unlocked"),
										new ("name", NAME, "Name of wallet", Flag.Optional)
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
						Arguments =	[new ("name", NAME, "Name of wallet", Flag.Optional)]};

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
						Arguments =	[new ("name", NAME, "Name of wallet", Flag.Optional),
									 new ("key", PRIVATEKEY, "Private key of account to import")]};

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
										new (p, FILEPATH, "A path to a source wallet file"),
									]};

		a.Execute = () =>	{
								var	b = File.ReadAllBytes(GetString(p));

								Api(new AddWalletApc {Raw = b});
		
								return a;
							};
		return a;
	}
}
