using System.Numerics;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class AssetsPage : Page
{
	public AssetsPage()
	{
	}

	public AssetsPage(Nexus nexus) : base(nexus)
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

			if(Nexus.Settings.Zone == Zone.Simulation)
			{
				Entity.Text = "user/f000";
			}
		}
	}

	private void Nets_Changed(object sender, EventArgs e)
	{
		//Search(Entity.SelectedItem.ToString());
	}

	private void Accounts_KeyDown(object sender, KeyEventArgs e)
	{
		if(!Start.Enabled)
			return;

		if(e.KeyCode == Keys.Enter)
		{
			Search(Entity.Text);
		}
	}

	void Search(string entity)
	{
		var f = new Flow(5000);

		Assets.Items.Clear();
		Message.Visible = true;
		Message.Text = "Loading...";
		Start.Enabled = false;

		var items = new ListViewItem();

		var net = Nets.Text;

		Task.Run(() => { 
							try
							{
								var e = Nexus.IccpLcpServer.Call<AddressTextToUniversalIccr>(null, net, new AddressTextToUniversalIcca {Text = entity}, f);

								foreach(var a in Nexus.IccpLcpServer.Call<HolderAssetsIccr>(null, net, new HolderAssetsIcca {Entity = e.Universal}, f).Assets)
								{
									var b = Nexus.IccpLcpServer.Call<AssetBalanceIccr>(null, net, new AssetBalanceIcca {Entity = e.Universal, Asset = a.Id}, f).Balance;
			
									var li = new ListViewItem(entity);
									li.SubItems.Add(a.Name);
									li.SubItems.Add(a.Units);
									li.SubItems.Add(b.ToString());
								}

								Invoke(() => {
												Assets.Items.AddRange(items);
												Message.Visible = false;
											 });
							}
							catch(CodeException ex)
							{
								Invoke(() => Message.Text = ex.Message);
							}
							catch(NexusException ex)
							{
								Invoke(() => Message.Text = ex.Message);
							}
							catch(OperationCanceledException ex)
							{
								Invoke(() => Message.Text = "Operation was canceled or timed out");
							}
							catch(FormatException ex)
							{
								Invoke(() =>	{ 
													Message.Visible = false;
													ShowError(ex.Message);
												});
							}
							finally
							{
								Invoke(() => Start.Enabled = true);
							}
						});

	}

	private void Start_Click(object sender, EventArgs e)
	{
		Search(Entity.Text);
	}
}
