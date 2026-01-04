using System.Numerics;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class AssetsPage : Page
{
	public AssetsPage()
	{
	}

	public AssetsPage(Nexus nexus, NnpIppClientConnection nnp) : base(nexus, nnp)
	{
		InitializeComponent();
	}

	public override void Open(bool first)
	{
		if(first)
		{
			Nets.Items.Insert(0, "rdn");
			Nets.SelectedIndex = 0;

			Message.Visible = false;

			if(Nexus.Settings.Zone == Zone.Local)
			{
				Entity.Text = "user/father0000";
			}
		}
	}

	private void Nets_Changed(object sender, EventArgs e)
	{
		Search(Entity.SelectedItem.ToString());
	}

	private void Accounts_KeyDown(object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter)
		{
			Search(Entity.Text);
		}
	}

	void Search(string entity)
	{
		var f = new Flow("");

		Assets.Items.Clear();
		Message.Visible = true;
		Message.Text = "Loading...";

		try
		{
			foreach(var a in Nnp.Call(new Nnc<HolderAssetsNna, HolderAssetsNnr>(new () {Net = Nets.Text, Entity = entity}), f).Assets)
			{
				var b = Nnp.Call(new Nnc<AssetBalanceNna, AssetBalanceNnr>(new () {Net = Nets.Text, Entity = entity, Name = a.Name}), f).Balance;
			
				var li = new ListViewItem(entity);
				li.SubItems.Add(a.Name);
				li.SubItems.Add(a.Units);
				li.SubItems.Add(b.ToString());
			
				Assets.Items.Add(li);
			}
			
			Message.Visible = false;
		}
		catch(CodeException ex)
		{
			Message.Text = ex.Message;
		}
		catch(NexusException ex)
		{
			Message.Text = ex.Message;
		}
		catch(FormatException ex)
		{
			Message.Visible = false;

			ShowError(ex.Message);
		}
	}

	private void Start_Click(object sender, EventArgs e)
	{
		Search(Entity.Text);
	}
}
