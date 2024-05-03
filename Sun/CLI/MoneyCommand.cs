using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: unt emit from {
	///							wallet = PATH 
	///							[password = PASSWORD] 
	///						 }
	///					amount = UNT
	///					to	 {
	///							account = ACCOUNT 
	///							[password = PASSWORD]
	///						 }
	///					[awaitconfirmation]
	///
	/// Usage: unt transfer from = ACCOUNT 
	///						[password = PASSWORD] 
	///						to = ACCOUNT 
	///						amount = UNT 
	/// </summary>
	///	<example>
	/// 
	/// money emit from{wallet=m:\UO\Team\Maximion\0x321D3AB8998c551Aeb086a7AC28635261AC66c00.mew} amount=1 by=0x001fea628d33830e5515e52fb7e3f9a009b24317
	/// 
	/// </example>
	
	


	public class MoneyCommand : Command
	{
		public const string Keyword = "money";

		public MoneyCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
				case "eee" :
		   		case "estimateethereumemission" :
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

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

					Workflow.Log?.Report($"   Estimated Tx Cost: {Web3.Convert.FromWeiToBigDecimal(f.Gas.Value * f.GasPrice.Value)} ETH");
// 
// 					var eid = 0;
// 
// 					try
// 					{
// 						eid = Rdc(new AccountRequest {Account = GetAccountAddress("by")}).Account.LastEmissionId + 1;
// 					}
// 					catch(EntityException ex) when (ex.Error == EntityError.NotFound)
// 					{
// 					}
// 
// 					Workflow.Log?.Report($"   Next Eid : {eid}");

					return f;
				}

		   		case "eie" :
		   		case "emitinethereum" :
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

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
																Gas = Web3.Convert.ToWei(GetString("gas")),
																GasPrice = Web3.Convert.ToWei(GetString("gasprice"))});

					Dump(t);

					return t;
				}

				case "eiu" :
		   		case "emitonultranet" :
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new Emission(Web3.Convert.ToWei(GetString("amount")), GetInt("eid"));
				}

// 		   		case "fe" :
// 		   		case "findemission" :
// 				{
// 					Workflow.CancelAfter(RdcTransactingTimeout);
// 
// 
// 					var e = Api<BigInteger>(new EmissionApc{Eid = (int)GetLong("eid"),
// 															By = GetAccountAddress("by"), 
// 															Await = GetAwaitStage(Args) });
// 
// 					if(e > 0)
// 						Workflow.Log?.Report(this, $"Amount: {Web3.Convert.FromWei(e)}");
// 					else
// 						throw new Exception("Not found");
// 
// 					return null;
// 				}

		   		case "t" : 
		   		case "transfer" : 
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new UntTransfer(AccountAddress.Parse(GetString("to")), Money.ParseDecimal(GetString("amount")));
				}

		   		case "c" : 
		   		case "cost" : 
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					var c = new CostApc{Years = [1, 5, 10], 
										DomainLengths = [1, 5, 10, 15], 
										Rate = GetMoney("rate", 1)};

					var r = Api<CostApc.Report>(c);

					Workflow.Log?.Report($"   Byte Per Day Rent    : {r.RentBytePerDay.ToDecimalString()}");
					Workflow.Log?.Report($"   Account One-time Fee : {r.RentAccount.ToDecimalString()}");
					Workflow.Log?.Report($"   Execution Unit       : {r.Exeunit.ToDecimalString()}");

					Workflow.Log?.Report($"");

					Dump(	r.RentDomain,
							["Domains Rent |", .. c.DomainLengths.Select(i => $"{i} chars")],
							[(o, i) => $"{c.Years[i]} year(s) |", .. c.DomainLengths.Select((x, li) => new Func<Money[], int, object>((j, i) => j[li].ToDecimalString()))]);

					Workflow.Log?.Report($"");

					Dump(	r.RentResource.Append(r.RentResourceForever),
							["Resource Rent", "Cost"],
							[(o, i) => i < r.RentResource.Length ? $"{c.Years[i]} year(s)" : "Forever", (o, i) => o.ToDecimalString()]);

					Workflow.Log?.Report($"");

					Dump(	r.RentResourceData.Append(r.RentResourceDataForever),
							["Resource Data Per Byte Rent", "Cost"],
							[(o, i) => i < r.RentResourceData.Length ? $"{c.Years[i]} year(s)" : "Forever", (o, i) => o.ToDecimalString()]);

					return r;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
