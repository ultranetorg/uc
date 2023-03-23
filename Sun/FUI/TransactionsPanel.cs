using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Sun.FUI
{
	public partial class TransactionsPanel : MainPanel
	{
		public TransactionsPanel(Core d, Vault vault) : base(d, vault)
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
				BindAccounts(Account);

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
				lock(Core.Lock)
				{
					Operations.Items.AddRange((e.Item.Tag as Transaction).Operations.Select((i) =>
																							{
																								var li = new ListViewItem(i.Id.ToString());
																								//li.Tag = i;
																								li.SubItems.Add(i.GetType().Name + ", " + i.Description);
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
	
				lock(Core.Lock)
				{
					var a = Net.AccountAddress.Parse(Account.Text);
					//var txs = Core.Transactions.Where(i => i.Signer == a);
					var txs = Core.Database.Accounts.SearchTransactions(a).OrderByDescending(i => i.Operations.Any() ? i.Operations.Max(o => o.Id) : 0);
	
					foreach(var i in txs)
					{
						var li = new ListViewItem(i.Payload.Round.Id.ToString());
	
						li.Tag = i;
	
						//li.SubItems.Add(i.Id.ToString());
						//li.SubItems.Add();
						//li.SubItems.Add(i.Successful ? "OK" : "Failed" /*i.Payload.Round.Confirmed ? "Confirmed" : "Confirming..."*/);
						li.SubItems.Add(i.Generator?.ToString());
		
						Transactions.Items.Add(li);
					}

					foreach(var i in txs.SelectMany(i => i.Operations))
					{
						var li = new ListViewItem(i.Id.ToString());
						li.SubItems.Add(i.GetType().Name + ", " + i.Description);
		
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
