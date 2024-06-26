using System.Windows.Forms;

namespace Uccs.Net.FUI
{
	public partial class FeeForm : Form, IFeeAsker
	{
		McvZone Zone;

		public FeeForm(McvZone zone)
		{
			Zone = zone;

			InitializeComponent();
		}

		private void send_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		public bool Ask(Net.Node sun, AccountAddress account, Operation operation)
		{
			from.Text = account.ToString();
			
			var t = new Transaction(){Expiration = 0};
			t.AddOperation(operation);
			//t.Sign(account, Zone.Cryptography.ZeroHash);

			//var f = sun.EstimateFee(t.Operations);
			//
			//fee.Text = f.ToString();

			if(ShowDialog() == DialogResult.OK)
			{
				return true;
			} 
			else
			{
				return false;
			}
		}
	}
}
