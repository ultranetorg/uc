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
	public partial class FlowControlForm : Form, IFlowControl
	{
		Dispatcher				Dispatcher;
		Operation				Operation;
		Log						IFlowControl.Log => Log.Log;
		CancellationTokenSource CancellationToken;

		public FlowControlForm(Dispatcher dispatcher, Log log, CancellationTokenSource cancellationTokenSource)
		{
			InitializeComponent();

			Dispatcher = dispatcher;
			CancellationToken = cancellationTokenSource;
			Log.Log = log;
		}

		private void Abort_Click(object sender, EventArgs e)
		{
			CancellationToken.Cancel();
			Close();
		}

		private void Exit_Click(object sender, EventArgs e)
		{
			Close();
		}

		public void SetOperation(Operation o)
		{
			Operation = o;
			Operation.FlowReport = this;
			Text = o.ToString();
		}

		public void StageChanged()
		{
			Log.Log.Report(this, "Processing stage changed", $"to '{Operation.Stage}'");
		}
	}
}
