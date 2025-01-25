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

								var w = Uos.Vault.CreateWallet();

								w.Encrypt(p);

								Report("Public Address - " + w.Address); 
								Report("Private Key    - " + w.Key.GetPrivateKeyAsBytes().ToHex());

								Api(new AddWalletApc {Raw = w.ToRaw()});

								return w.Key;
							};
		return a;
	}

	public CommandAction Unlock()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "u";

		a.Help = new() {Description = "Unlocks an existing wallet making it available for signing transactions",
						Syntax = $"{Keyword} {a.NamesSyntax} {AA} password={PASSWORD}",

						Arguments =	[
										new ("<first>", "Account address of wallet to unlock"),
										new ("password", "A password of a wallet to be unlocked")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {AA.Example} password={PASSWORD.Example}")
									]};

		a.Execute = () =>	{
								Api(new UnlockWalletApc{Account = AccountAddress.Parse(Args[0].Name), 
														Password = GetString("password")});
								return null;
							};
		return a;
	}

	public CommandAction Import()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "i";

		a.Help = new() {Description = "Imports a new account using the provided private key of an account or a wallet file.",
						Syntax = $"{Keyword} {a.NamesSyntax} (privatekey={PRIVATEKEY} password={PASSWORD}) | wallet={FILEPATH}",

						Arguments =	[
										new ("privatekey", "A private key of an account for which a wallet is created"),
										new ("wallet", "A path to a source wallet file"),
										new ("password", "When privatekey is specified, a password that will be used to encrypt the newly created wallet")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} privatekey={PRIVATEKEY.Examples} password={PASSWORD.Example}"),
										new (null, $"{Keyword} {a.Name} wallet={DIRPATH}\\wallet.sunwe")
									]};

		a.Execute = () =>	{
								byte[] b;
		
								if(Has("privatekey"))
								{
									string p = GetString("password", null);

									if(p == null)
									{
										Uos.PasswordAsker.Create(Vault.PasswordWarning);
										p = Uos.PasswordAsker.Password;
									}

									var w = Uos.Vault.CreateWallet(new AccountKey(GetBytes("privatekey")));
									w.Encrypt(p);
									b = w.ToRaw();
								}
								else if(Has("wallet"))
								{
									b = File.ReadAllBytes(GetString("wallet"));
								}
								else
									throw new SyntaxException("'privatekey' or 'wallet' must be provided");

								Api(new AddWalletApc {Raw = b});

								Report("Account Address - " + Uos.Vault.CreateWallet(b).Address);
		
								return a;
							};
		return a;
	}
}
