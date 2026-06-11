using System.Data;
using System.Windows.Forms;

namespace Uccs.Mcv.FUI;

#pragma warning disable IDE0055

public partial class TransferPanel : McvPanel
{
	McvNode						Node;
	Dictionary<byte, string>	Tables;	

	User						FromUser;
	User						ToUser;
	byte[]						Session;

	public TransferPanel(McvNode node)
	{
		Node = node;

		InitializeComponent();
	}

	public override void Open(bool first)
	{
		if(first)
		{
			if(Node.Iccp == null)
			{
				Enabled = false;
				return;
			}

			FromAs.DataSource = new string[] {"Id", "Name"};
			ToWhat.DataSource = new string[] {"Id", "Name"};

			FromAs.SelectedIndex = 1;
			ToWhat.SelectedIndex = 1;

			Asset.Enabled = false;
			FromClass.Enabled = false;
			ToClass.Enabled = false;
			
			SetLoading(Asset);
			SetLoading(FromClass);
			SetLoading(ToClass);

			Task.Run(() =>	{
								var f = new Flow();

								Tables = Node.Peering.Call(new InfoPpc(), f).Tables;

								var cs = Node.Iccp.Call<HolderClassesIccr>(null, Node.Net.Address, new HolderClassesIcca{}, f).Classes;

								BeginInvoke(() =>	{
														FromClass.Items.Clear();
														ToClass.Items.Clear();

														foreach(var i in cs)
														{
															FromClass.Items.Add(i);
															ToClass.Items.Add(i);

															FromClass.Enabled = true;
															ToClass.Enabled = true;

															FromClass.SelectedIndex = 0;
															ToClass.SelectedIndex = 0;
														}
													});
								
								var assets = Node.Iccp.Call<HolderAssetsIccr>(null, Node.Net.Address, new HolderAssetsIcca {}, f).Assets;
								
								Invoke(() => {
											 	Asset.Items.Clear();
											 
											 	foreach(var a in assets)
											 		Asset.Items.Add(a);
											 
											 	Asset.Enabled = true;
											 });
							});
		}
	}
	
	private void SetLoading(ComboBox box)
	{
		box.Items.Clear();
		box.Items.Add("Loading...");
		box.SelectedIndex = 0;
	}
	
	private void Asset_SelectionChangeCommitted(object sender, EventArgs e)
	{
		try
		{
			RefreshBalance();
		}
		catch(Exception ex)
		{
		}
	}

	void RefreshBalance()
	{
		Balance.Text = "...";

		try
		{
			if(FromAs.Text == "Id")
			{
				if(!AutoId.TryParse(From.Text, out var id))
					return;

				FromUser = Node.Peering.Call(new UserPpc {Id = id}, Node.Flow).User;
			} 
			else
			{
				FromUser = Node.Peering.Call(new UserPpc {Name = From.Text}, Node.Flow).User;
			}
		}
		catch(Exception ex)
		{
			return;
		}

		if(Asset.SelectedItem is Asset a)
		{
			Task.Run(() => BeginInvoke(() =>	{
													try
													{
														Balance.Text = Node.Iccp.Call<AssetBalanceIccr>(null,
																										Node.Net.Address,
																										new AssetBalanceIcca
																										{
																											Entity = IccpEntityAddress.ToBytes(Tables.First(i => i.Value == FromClass.Text).Key, FromUser.Id),
																											Asset = a.Id
																										},
																										Node.Flow).Balance.ToString();
													}
													catch(Exception ex)
													{
														
													}
												}));
		}

	}

	private void Any_Changed(object sender, EventArgs e)
	{
		Transfer.Enabled =	!string.IsNullOrEmpty(From.Text) &&
							!string.IsNullOrEmpty(To.Text) &&
							Asset.SelectedItem is Asset;
	}

	private void FromId_TextChanged(object sender, EventArgs e)
	{
		RefreshBalance();
	}
	
	private void Transfer_Click(object sender, EventArgs e)
	{
		try
		{
			ToUser = Node.Peering.Call(new UserPpc {Name = To.Text}, Node.Flow).User;
	
			var s = Node.Peering.FindSession(FromUser.Name)
					??
					Node.Peering.CreateSession(Node.NexusSettings.Name, FromUser.Name);
	
			var a = Asset.SelectedItem as Asset;
	
			Node.Peering.Transact([new UtilityTransfer( Tables.First(i => i.Value == FromClass.Text).Key,
														FromUser.Id,
														Tables.First(i => i.Value == ToClass.Text).Key,
														ToUser.Id,
														a.Id[0] == 0 && a.Id[1] == 0 ? long.Parse(Amount.Text) : 0,
														a.Id[0] == 0 && a.Id[1] == 1 ? long.Parse(Amount.Text) : 0,
														a.Id[0] == 1 ? long.Parse(Amount.Text) : 0)], 
									FromUser.Name, 
									null, 
									s.Session, 
									ActionOnResult.RetryUntilConfirmed,
									new Flow());
		}
		catch(Exception ex)
		{
			ShowError(ex.Message);
		}
	}

	bool fit = false;

	public override void PeriodicalRefresh()
	{
		if(!Enabled || Tables == null)
			return;

		try
		{
			Transactions.Items.Clear();

			Transaction[] txs;

			lock(Node.Peering.Lock)
				txs = Node.Peering.OutgoingTransactions.Where(i => i.Operations[0] is UtilityTransfer).ToArray();
			
			foreach(var i in txs)
			{
				var li = new ListViewItem(i.Tag.ToHex()) {Tag = i};
					
				var o = i.Operations[0] as UtilityTransfer;

				li.SubItems.Add(Tables[o.From.Table]);
				li.SubItems.Add(o.From.Id.ToString());
				li.SubItems.Add(Tables[o.To.Table]);
				li.SubItems.Add(o.To.Id.ToString());
				li.SubItems.Add(o.Spacetime.ToString());
				li.SubItems.Add(o.Energy.ToString());
				li.SubItems.Add(o.EnergyNext.ToString());
				li.SubItems.Add(i.Status.ToString());
				li.SubItems.Add(i.Error);
				
				Transactions.Items.Add(li);
			}

			if(!fit && Transactions.Items.Count > 0)
			{	
				Transactions.ResizeColumnsToFit();
				fit = true;
			}
		}
		catch(Exception ex)
		{
		}
	}

}

#pragma warning restore IDE0055