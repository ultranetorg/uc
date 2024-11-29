using System.Data;
using System.Windows.Forms;

namespace Uccs.Net.FUI
{
	public partial class TransactionsPanel : MainPanel
	{
		McvNode Node;
		
		public TransactionsPanel(McvNode mcv)
		{
			InitializeComponent();

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

				Search();
			}
		}

		private void search_Click(object sender, EventArgs e)
		{
			Search();
		}

		private void account_SelectedIndexChanged(object sender, EventArgs e)
		{
			Search();
		}

		private void account_SelectionChangeCommitted(object sender, EventArgs e)
		{
			//	Search();
		}

		private void Transactions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			Operations.Items.Clear();

			if(e.IsSelected)
			{
				lock(Node.Mcv.Lock)
				{
					Operations.Items.AddRange((e.Item.Tag as Transaction).Operations.Select((i) =>	{
																										var li = new ListViewItem(i.ToString());
																										//li.Tag = i;
																										//li.SubItems.Add(i.GetType().Name + ", " + i.Description);
																										return li;
																									})
																									.ToArray());
				}
			}
		}

		void Search()
		{
			try
			{
				Transactions.Items.Clear();
				Operations.Items.Clear();

				lock(Node.Mcv.Lock)
				{
					var a = AccountAddress.Parse(Account.Text);
					//var txs = Core.Transactions.Where(i => i.Signer == a);
					var txs = Node.Mcv.Accounts.SearchTransactions(a).OrderByDescending(i => i.Nid);

					foreach(var i in txs)
					{
						var li = new ListViewItem(i.Nid.ToString()) {Tag = i};
						li.SubItems.Add(i.Round.Id.ToString());
						//li.SubItems.Add(i.Member?.ToString());

						Transactions.Items.Add(li);
					}

					foreach(var i in txs.SelectMany(i => i.Operations))
					{
						var li = new ListViewItem(i.ToString());
						//li.SubItems.Add(i.GetType().Name + ", " + i.Description);

						Operations.Items.Add(li);
					}
				}

				if(Transactions.Items.Count == 0)
				{
					Transactions.Items.Add("No results");
				}
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}
	}
}
