using Uccs.Nexus;

namespace Uccs.Iam.FUI;

public partial class IamForm : Form
{
	public IamForm()
	{
		InitializeComponent();
	}

	public IamForm(Iam iam)
	{
		InitializeComponent();

		WalletsAndAccounts.Tag = new WalletsPage(iam);
		Sessions.Tag = new SessionsPage();
		Assets.Tag = new AssetsPage();

		WalletsAndAccounts.Checked = true;
	}
	
	private void radioButton4_CheckedChanged(object sender, EventArgs e)
	{
		var p = (sender as Control).Tag as Page;

		foreach(Control i in Controls)
			if(i is Page j)
			{
				j.Close();
				Controls.Remove(i);
			}

		if(p != null)
		{
			p.Location	= Place.Location;
			p.Size		= Place.Size;
			p.Anchor	= AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			Controls.Add(p);
			Controls.SetChildIndex(p, 0);

			p.Open(p.First);

			p.First = false;
		}
	}
}
