using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Uccs.Net;

namespace Uccs.Uos
{
	public class WalletCommand : UosCommand
	{
		public const string Keyword = "wallet";

		public WalletCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
		{
			Actions =	[
							new ()
							{
								Names = ["c", "create"],

								Help = new Help
								{ 
									Title = "CREATE",
									Description = "Used to create a new account and its wallet.",
									Syntax = "wallet c|create [password=PASSWORD]",

									Arguments =
									[
										new ("password", "A password that is used to encrypt the newly created wallet")
									],

									Examples =
									[
										new (null, "wallet create password=TheSuccessorOfTheWeb")
									]
								},

								Execute = () =>	{
													string p = GetString("password", null);

													if(p == null)
													{
														Uos.PasswordAsker.Create(Vault.PasswordWarning);
														p = Uos.PasswordAsker.Password;
													}

													var k = AccountKey.Create();

													Report("Public Address - " + k.ToString()); 
													Report("Private Key    - " + k.Key.GetPrivateKeyAsBytes().ToHex());

													Api(new AddWalletUosApc {Wallet = Uos.Vault.Cryptography.Encrypt(k, p)});
													Api(new SaveWalletUosApc {Account = k});

													return k;
												}
							},

							new ()
							{
								Names = ["u", "unlock"],

								Help = new Help
								{ 
									Title = "UNLOCK",
									Description = "Unlocks an existing wallet making it available for signing transactions",
									Syntax = "wallet u|unlock UAA password=PASSWORD",

									Arguments =
									[
										new ("<first>", "Account address of wallet to unlock"),
										new ("password", "A password of a wallet to be unlocked")
									],

									Examples =
									[
										new (null, "wallet u password=TheSuccessorOfTheWeb")
									]
								},

								Execute = () =>	{
													Api(new UnlockWalletApc{Account = AccountAddress.Parse(Args[0].Name), 
																			Password = GetString("password")});
													return null;
												}
							},

							new ()
							{
								Names = ["i", "import"],

								Help = new Help
								{ 
									Title = "IMPORT",
									Description = "Imports a new account using the provided private key of an account or a wallet file.",
									Syntax = "wallet i|import privatekey=PRIVATEKEY [password=PASSWORD] | wallet=PATH",

									Arguments =
									[
										new ("privatekey", "A private key of an account for which a wallet is created"),
										new ("wallet", "A path to a source wallet file"),
										new ("password", "When privatekey is specified, a password that will be used to encrypt the newly created wallet")
									],

									Examples =
									[
										new (null, "wallet import privatekey=f5eb914b0cdf95fb3df9bcf7e3686cb16d351edf772e577dd6658f841f51b848 password=TheSuccessorOfTheWeb"),
										new (null, "wallet import wallet=C:\\wallet.sunwe")
									]
								},

								Execute = () =>	{
													byte[] w;
			
													if(Has("privatekey"))
													{
														string p = GetString("password", null);

														if(p == null)
														{
															Uos.PasswordAsker.Create(Vault.PasswordWarning);
															p = Uos.PasswordAsker.Password;
														}

														var k = new AccountKey(GetBytes("privatekey"));
														w = Uos.Vault.Cryptography.Encrypt(k, p);
													}
													else if(Has("wallet"))
													{
														w = File.ReadAllBytes(GetString("wallet"));
													}
													else
														throw new SyntaxException("'privatekey' or 'wallet' must be provided");

													var a = Uos.Vault.Cryptography.AccountFromWallet(w);

													Api(new AddWalletUosApc {Wallet = w});
													Api(new SaveWalletUosApc {Account = a});

													Report("Account Address - " + a.ToString());
			
													return a;
												}
							},
						];
		}
	}
}
