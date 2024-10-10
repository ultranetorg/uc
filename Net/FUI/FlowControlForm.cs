using System.Windows.Forms;

namespace Uccs.Net.FUI
{
	public partial class FlowControlForm : Form
	{
		Uccs.Net.TcpPeering				Sun;
		Operation			Operation;
		Flow			Vizor;

		public FlowControlForm(Uccs.Net.TcpPeering sun, Flow vizor)
		{
			InitializeComponent();

			Sun = sun;
			Vizor = vizor;
			Logview.Log = Vizor.Log;
		}

		private void Abort_Click(object sender, EventArgs e)
		{
			Vizor.Abort();
			Close();
		}

		private void Exit_Click(object sender, EventArgs e)
		{
			Close();
		}

		//public void SetOperation(Operation o)
		//{
		//	Operation = o;
		//	Operation.FlowReport = this;
		//	Text = o.ToString();
		//}
		//
		//public void StageChanged()
		//{
		//	Logview.Log.Report(this, "Delegation stage changed", $"Delegation={Operation.Delegation}");
		//}
	}
}
