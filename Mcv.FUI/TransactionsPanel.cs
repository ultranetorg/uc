using System.Data;
using System.Windows.Forms;

namespace Uccs.Mcv.FUI;

public partial class TransactionsPanel : McvPanel
{
	McvNode Node;
	
	public TransactionsPanel(McvNode node)
	{
		InitializeComponent();

		Node = node;
		/*
		foreach(DataGridViewColumn i in members.Columns)
		{
			i.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
		}*/
	}

	public override void Open(bool first)
	{
		if(first)
		{
			//BindAccounts(Account);

			DoReload();
		}
	}

	private void Refresh_Click(object sender, EventArgs e)
	{
		DoReload();
	}

	private void Transactions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		Operations.Items.Clear();

		if(e.IsSelected)
		{
			lock(Node.Peering.Lock)
			{
				var t = e.Item.Tag as Transaction;
				Operations.Items.AddRange(t.Operations.Select((i) => {
																	 	var li = new ListViewItem(i.ToString());
																	 	//li.Tag = i;
																	 	//li.SubItems.Add(i.GetType().Name + ", " + i.Description);
																	 	return li;
																	 })
																	 .ToArray());
				
				Log.Lines = t.Flow.Log.Messages.Select(i => i.ToString()).ToArray();
			}
		}
	}

	void DoReload()
	{
		try
		{
			Transactions.Items.Clear();
			Operations.Items.Clear();
			Log.Clear();

			lock(Node.Peering.Lock)
			{
				foreach(var i in Node.Peering.OutgoingTransactions)
				{
					var li = new ListViewItem(i.Tag.ToHex()) {Tag = i};
					li.SubItems.Add(i.Status.ToString());
					li.SubItems.Add(i.User);
					li.SubItems.Add(i.Nonce.ToString());
					li.SubItems.Add(i.Expiration.ToString());
					li.SubItems.Add(i.Operations.Length.ToString());
					li.SubItems.Add(i.Operations[0].ToString());
					li.SubItems.Add(i.Error);
					li.SubItems.Add((i.Ppi as Peer)?.EP.ToString());
				
					Transactions.Items.Add(li);
				}

				if(Transactions.Items.Count > 0)
					Transactions.Items[0].Selected = true;
			}
		}
		catch(Exception ex)
		{
			ShowError(ex.Message);
		}
	}
}
