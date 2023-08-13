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

namespace Uccs.Sun.FUI
{
	public partial class FlowControlForm : Form
	{
		Net.Sun				Sun;
		Operation			Operation;
		Workflow			Vizor;

		public FlowControlForm(Net.Sun sun, Workflow vizor)
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
