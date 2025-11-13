using Uccs.Nexus;

namespace Uccs.Nexus.Windows;

public partial class IamForm : Form
{
	public IamForm()
	{
		InitializeComponent();
	}

	public IamForm(Nexus nexus)
	{
		InitializeComponent();

		WalletsAndAccounts.Tag = new WalletsPage(nexus);
		Sessions.Tag = new SessionsPage(nexus);
		Assets.Tag = new AssetsPage(nexus);
		Transfer.Tag = new TransferPage(nexus);

		WalletsAndAccounts.Checked = true;
	}
	
	private void radioButton_CheckedChanged(object sender, EventArgs e)
	{
		var r = (sender as RadioButton);
		var p = (sender as Control).Tag as Page;

		if(!r.Checked)
		{
			foreach(Control i in Controls)
				if(i is Page j)
				{
					j.Close();
					Controls.Remove(i);
				}
		}
		else 
		{
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

			Transfer.Visible = p is AssetsPage || p is TransferPage;
		}
	
	}
}
