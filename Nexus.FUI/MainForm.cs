using System.Windows.Forms;

namespace Uccs.Nexus.FUI;

public partial class MainForm : Form
{
	Nexus							Nexus;
	System.Windows.Forms.Timer		Timer = new ();

	public MainForm(Nexus nexus)
	{
		Nexus = nexus;
		AutoScaleMode = AutoScaleMode.Inherit;

		InitializeComponent();

		MinimumSize = Size;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		Timer.Tick += RefreshInfo;
		Timer.Interval = 1000;
		Timer.Start();
	}

	private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		Timer.Stop();
		Timer.Tick -= RefreshInfo;
	}

	void RefreshInfo(object myObject, EventArgs myEventArgs)
	{
		//lock(Uos.Lock)
		{
			Text = $"Uos";
		}

		//foreach(var i in Controls)
		//	if(i is MainPanel p)
		//		p.PeriodicalRefresh();
	}
}
