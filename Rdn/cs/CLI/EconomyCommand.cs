using System.Numerics;

namespace Uccs.Rdn.CLI
{
	public class EconomyCommand : RdnCommand
	{
		public const string Keyword = "economy";

		public EconomyCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{

			var fromA =	new Help.Argument("from (A)", "Using wallet file")
						{
							Arguments =
							[
								new ("wallet", "Keystore file of Ethereum account where funds are debited from"),
								new ("password", "A password to access wallet file"),
							]
						};

			var fromB =	new Help.Argument("from (B)", "Using private key")
						{
							Arguments =
							[
								new ("key", "Private key of Ethereum account where funds are debited from"),
							]
						};


			Actions =	[
#if ETHEREUM
							new ()
							{
								Names = ["eee", "estimateethereumemission"],

								Help = new Help
								{ 
									Title = "ESTIMATE ETHEREUM EMISSION",
									Description = "Used to estimate gas and gas price of emission transaction on the Ethereum side",
									Syntax = "money eee|estimate-ethereum-emission from{key=PRIVATEKEY | wallet=PATH [password=PASSWORD]}",

									Arguments =
									[
										fromA,
										fromB,
										new ("amount", "Amount of ETH to be converted into UNT")
									],

									Examples =
									[
										new (null, "money eee from{wallet=C:\\aaaabbbbccccddddd111122223333.json password=thesuccessoroftheweb}")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

													Nethereum.Web3.Accounts.Account from;

													if(Has("from/key"))
													{
														from = new Nethereum.Web3.Accounts.Account(GetString("from/key"), Program.Zone.EthereumNetwork);
													}
													else
													{
														string p;

														if(Has("from/password"))
														{
															p = GetString("from/password");
														}
														else
														{
															Program.PasswordAsker.Ask(GetString("from/wallet"));
															p = Program.PasswordAsker.Password;
														}

														from = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(File.ReadAllText(GetString("from/wallet")), 
																												p, 
																												new BigInteger((int)Program.Zone.EthereumNetwork));
													}
			
													var f = Api<EmitFunction>(new EstimateEmitApc  {FromPrivateKey = from.PrivateKey.HexToByteArray(),
																									Wei = Web3.Convert.ToWei(GetString("amount"))});
													Dump(f);

													Report($"Estimated Tx Cost: {Web3.Convert.FromWeiToBigDecimal(f.Gas.Value * f.GasPrice.Value)} ETH");
								// 
								// 					var eid = 0;
								// 
								// 					try
								// 					{
								// 						eid = Rdc(new AccountRequest {Account = GetAccountAddress("signer")}).Account.LastEmissionId + 1;
								// 					}
								// 					catch(EntityException ex) when (ex.Error == EntityError.NotFound)
								// 					{
								// 					}
								// 
								// 					Workflow.Log?.Report($"   Next Eid : {eid}");

													return f;
												}
							},

							new ()
							{
								Names = ["eie", "emitinethereum"],

								Help = new Help
								{ 
									Title = "EMIT IN ETHEREUM",
									Description = "Places a special transaction on Ethereum side which tells that specified amount of ETH is burned in exchange of crediting corresponding amount of UNT to a specified account on ULTRANET side",
									Syntax = "money eie|emit-in-ethereum from{key=PRIVATEKEY | wallet=PATH [password=PASSWORD]} eid=INT gas=INT gasprice=INT amount=UNT to=UAA",

									Arguments =
									[
										fromA,
										fromB,
										new ("eid", "Emission sequence identifier, zero-based"),
										new ("gas", "Amount of gas required, use estimate-ethereum-emission command to get"),
										new ("gasprice", "Gas price, use estimate-ethereum-emission command to get"),
										new ("amount", "Amount of ETH to be converted into UNT"),
										new ("to", "Ultranet account address where UNTs are credited to")
									],

									Examples =
									[
										new (null, "money eie from{wallet=C:\\aaaabbbbccccddddd111122223333.json password=thesuccessoroftheweb} amount=1 to=0x0000fffb3f90771533b1739480987cee9f08d754 eid=0 gas=77096 gasprice=1984406409")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

													Nethereum.Web3.Accounts.Account from;

													if(Has("from/key"))
													{
														from = new Nethereum.Web3.Accounts.Account(GetString("from/key"), Program.Zone.EthereumNetwork);
													}
													else
													{
														string p;

														if(Has("from/password"))
														{
															p = GetString("from/password");
														}
														else
														{
															Program.PasswordAsker.Ask(GetString("from/wallet"));
															p = Program.PasswordAsker.Password;
														}

														from = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(File.ReadAllText(GetString("from/wallet")), 
																												p, 
																												new BigInteger((int)Program.Zone.EthereumNetwork));
													}
			
													var t = Api<TransactionReceipt>(new EmitApc{FromPrivateKey = from.PrivateKey.HexToByteArray(),
																								To = GetAccountAddress("to"),
																								Eid = GetInt("eid"),
																								Wei = Web3.Convert.ToWei(GetString("amount")),
																								Gas = BigInteger.Parse(GetString("gas")),
																								GasPrice = BigInteger.Parse(GetString("gasprice"))});

													Dump(t);

													return t;
												}
							},

							new ()
							{
								Names = ["eiu", "emitonultranet"],

								Help = new Help
								{ 
									Title = "EMIT IN ULTRANET",
									Description = "Places a special transaction on ULTRANET side which tells the network to retrieve emission info from the Ethereum side, check it and if everything is correct, credit a specified account with a corresponding amount of UNT",
									Syntax = "money eiu|emit-in-ultranet from{key=PRIVATEKEY | wallet=PATH [password=PASSWORD]} eid=EID amount=UNT signer=UAA",

									Arguments =
									[
										fromA,
										fromB,
										new ("eid", "Emission sequence identifier, zero-based"),
										new ("amount", "Amount of ETH to be verified and converted into UNT"),
										new ("signer", "Ultranet account address where UNTs are credited to")
									],

									Examples =
									[
										new (null, "money eiu from{wallet=C:\\aaaabbbbccccddddd111122223333.json password=thesuccessoroftheweb} amount=1000 signer=0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

													return new Immission(Web3.Convert.ToWei(GetString("amount")), GetInt("eid"));
												}
							},

							new ()
							{
								Names = ["fe", "findemission"],

								Help = new Help
								{ 
									Title = "FIND EMISSION",
									Description = "Requires Ethereum provider is configured. Retrieves an amount of ETH of existing emission transaction on the Ethereum side.",
									Syntax = "money fe|findemission eid=INT signer=UAA",

									Arguments =
									[
										new ("eid", "Emission sequence Id. First emission has eid=0, next emission has eid=1, and so on."),
										new ("signer", "Address of destination Ultranet account")
									],

									Examples =
									[
										new (null, "money findemission eid=0 signer=0x4C05950D7B413B2FBCF6C5B7858C4CDB0736E2DE")
									]		
								},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcTransactingTimeout);
												
												
													var e = Api<BigInteger>(new EmissionApc{Eid = (int)GetLong("eid"),
																							By = GetAccountAddress("signer"), 
																							Await = GetAwaitStage(Args) });
												
													if(e > 0)
														Flow.Log?.Report(this, $"Amount: {Web3.Convert.FromWei(e)}");
													else
														throw new Exception("Not found");
												
													return null;
 												}
							},
#endif

							new ()
							{
								Names = ["c", "cost"],

								Help = new Help { 
													Title = "COST",
													Description = "Gets information about current cost of various ULTRANET resources.",
													Syntax = $"{Keyword} c|cost",

													Arguments = [],

													Examples =	[
																	new (null, $"{Keyword} cost")
																]
												},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

													var c = new CostApc{Years = [1, 5, 10], 
																		DomainLengths = [1, 5, 10, 15], 
																		Rate = GetMoney("rate", 1)};

													var r = Api<CostApc.Return>(c);

													//Report($"Byte Per Day Rent    : {r.RentBytePerDay.ToDecimalString()}");
													Report($"Account One-time Bye-Year Fee : {r.RentAccount.ToString()}");
													//Report($"Execution Unit       : {r.Exeunit.ToDecimalString()}");

													Report($"");

													Dump(	r.RentDomain,
															["Domains Rent |>", .. c.DomainLengths.Select(i => $"{i} chars>")],
															[(o, i) => $"{c.Years[i]} year(s) |", .. c.DomainLengths.Select((x, li) => new Func<Unit[], int, object>((j, i) => j[li].ToString()))]);

													Report($"");

													Dump(	r.RentResource.Append(r.RentResourceForever),
															["Resource Rent>", "Cost>"],
															[(o, i) => i < r.RentResource.Length ? $"{c.Years[i]} year(s)" : "Forever", (o, i) => o.ToString()]);

													Report($"");

													Dump(	r.RentResourceData.Append(r.RentResourceDataForever),
															["Resource Data Per Byte Rent>", "Cost>"],
															[(o, i) => i < r.RentResourceData.Length ? $"{c.Years[i]} year(s)" : "Forever", (o, i) => o.ToString()]);

													return r;
												}
							},
						];
		}
	}
}
