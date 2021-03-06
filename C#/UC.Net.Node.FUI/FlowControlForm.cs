using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class FlowControlForm : Form
	{
		Core				Core;
		Operation			Operation;
		Flowvizor			Vizor;

		public FlowControlForm(Core core, Flowvizor vizor)
		{
			InitializeComponent();

			Core = core;
			Vizor = vizor;
			Logview.Log = Vizor.Log;
		}

		private void Abort_Click(object sender, EventArgs e)
		{
			Vizor.Cancellation.Cancel();
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
