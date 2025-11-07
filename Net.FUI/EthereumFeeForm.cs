using System.Windows.Forms;

namespace Uccs.Net.FUI;

	public partial class EthereumFeeForm : Form
	{
//		Web3			Web3;
//		ContractHandler	Contract;
//
//		public EthereumFeeForm()
//		{
//			InitializeComponent();
//		}
//
		private void send_Click(object sender, EventArgs e)
		{
//			DialogResult = DialogResult.OK;
//			Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
//			DialogResult = DialogResult.Cancel;
//			Close();
		}

		private void gas_TextChanged(object sender, EventArgs e)
		{
//			var wei = BigInteger.Parse(gas.Text) * Web3.Convert.ToWei(BigInteger.Parse(gasprice.Text), Nethereum.Util.UnitConversion.EthUnit.Gwei);
//
//			eth.Text = Web3.Convert.FromWeiToBigDecimal(wei).ToString();
		}
//
//		public bool Ask<F>(Web3 web3, ContractHandler c, string acc, F f, Log log) where F : FunctionMessage, new()
//		{
//			Web3 = web3;
//			Contract = c;
//
//			gas_TextChanged(this, EventArgs.Empty);
//
//			f.Gas = null;
//			f.GasPrice = null;
//
//			var bal		= Web3.Eth.GetBalance.SendRequestAsync(acc).Result;
//			var gp		= Web3.Eth.GasPrice.SendRequestAsync().Result.Value;
//			var g		= Contract.EstimateGasAsync(f).Result.Value;
//
//			account.Text = acc;
//			balance.Text = Web3.Convert.FromWeiToBigDecimal(bal.Value).ToString(); 
//			gasprice.Text = (gp/1000000000).ToString();
//			gas.Text = g.ToString();
//
//			return ShowDialog() == DialogResult.OK;
//		}
//
//		public BigInteger Gas { get => BigInteger.Parse(gas.Text); }
//		public BigInteger GasPrice { get => Web3.Convert.ToWei(BigInteger.Parse(gasprice.Text), Nethereum.Util.UnitConversion.EthUnit.Gwei); }
//
	}

