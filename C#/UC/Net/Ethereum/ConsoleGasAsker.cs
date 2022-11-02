using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace UC.Net.Ethereum
{
	public class ConsoleGasAsker : IGasAsker
	{
		public BigInteger Gas		{ get; protected set; }
		public BigInteger GasPrice  { get; protected set; }

		public ConsoleGasAsker()
		{
		}

		public bool Ask<F>(Web3 web3, ContractHandler contact, string acc, F f, Log log) where F : FunctionMessage, new()
		{
			f.Gas = null;
			f.GasPrice = null;

			var balance = web3.Eth.GetBalance.SendRequestAsync(acc).Result;
			GasPrice	= web3.Eth.GasPrice.SendRequestAsync().Result.Value;
			Gas			= contact.EstimateGasAsync(f).Result.Value;
			
			var wei = Gas * Web3.Convert.ToWei(GasPrice, Nethereum.Util.UnitConversion.EthUnit.Gwei);
			var cost = Web3.Convert.FromWeiToBigDecimal(wei);

			log?.Report(this, "Ethereum", new[]{"Estimations",
												$"Balance       : {Web3.Convert.FromWeiToBigDecimal(balance)} ETH",
												$"Estimated Gas : {Gas} WEI",
												$"Gas Price     : {GasPrice / Web3.Convert.GetEthUnitValue(Nethereum.Util.UnitConversion.EthUnit.Gwei)} GWEI",
												$"Cost          : {cost} ETH"});


			
			var m = $"Press 'Enter' to confirm the payment of {cost} ETH: ";
			Console.Write(m);

			var c = Console.ForegroundColor;

			var confirmed = Console.ReadKey().Key == ConsoleKey.Enter;

			if(confirmed)
			{
				Console.CursorLeft += m.Length;
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Confirmed");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("Canceled");
			}

			Console.WriteLine();
			Console.ForegroundColor = c;

			return confirmed;
		}
	};
}
