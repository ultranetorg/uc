#if ETHEREUM
using System.Numerics;
using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;

namespace Uccs.Rdn
{
	public interface IGasAsker
	{
		bool		Ask<F>(Web3 web3, ContractHandler c, string account, F f, Log log) where F : FunctionMessage, new();
		BigInteger	Gas { get; }
		BigInteger	GasPrice { get; } 
	}

	public class SilentGasAsker : IGasAsker
	{
		public BigInteger Gas { get; set; }
		public BigInteger GasPrice { get; set; }

		public SilentGasAsker()
		{
		}

		public bool Ask<F>(Web3 web3, ContractHandler contract, string acc, F f, Log log) where F : FunctionMessage, new()
		{
			f.Gas = null;
			f.GasPrice = null;

			var bal		= web3.Eth.GetBalance.SendRequestAsync(acc).Result;
			var gp		= web3.Eth.GasPrice.SendRequestAsync().Result.Value;
			var g		= contract.EstimateGasAsync(f).Result.Value;

			GasPrice = gp;
			Gas = g;

			log?.Report(this, "Ethereum", [	"Estimations",
											$"Balance       : {Web3.Convert.FromWeiToBigDecimal(bal)} ETH",
											$"Estimated Gas : {Gas} WEI",
											$"Gas Price     : {GasPrice / Web3.Convert.GetEthUnitValue(Nethereum.Util.UnitConversion.EthUnit.Gwei)} GWEI",
											$"Cost          : {Web3.Convert.FromWeiToBigDecimal(g * gp)} ETH"]);

			return true;
		}
	}
}
#endif
