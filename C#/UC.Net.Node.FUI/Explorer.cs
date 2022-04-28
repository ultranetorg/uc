using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net.Node.FUI
{
	public partial class ExplorerPanel : MainPanel
	{
		public ExplorerPanel(Core d, Vault vault) : base(d, vault)
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
				lock(Core.Lock)
				{
					Core.Chain.BlockAdded += (b) => Round.Maximum = Core.Chain.LastNonEmptyRound.Id;

					//Rounds.Items.AddRange(Enumerable.Range(0, Core.Chain.LastNonEmptyRound.Id).OrderByDescending(i => i).Select(i => new ListViewItem(i.ToString())).ToArray());
					Round.Minimum = 0;
					Round.Maximum = Core.Chain.LastNonEmptyRound.Id;
					Round.Value = Core.Chain.LastNonEmptyRound.Id;
				}
			}
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			Blocks.Items.Clear();
			Transactions.Items.Clear();
			Operations.Items.Clear();

			lock(Core.Lock)
			{
				var r =  Core.Chain.FindRound((int)Round.Value);
				InfoValues.Text =	(r.Confirmed ? "Confirmed " : "") + (r.Voted ? "Voted " : "") + "\n" + 
									r.Time + "\n" + 
									(r.Hash != null ? Hex.ToHexString(r.Hash) : null) + "\n" +
									string.Join(", ", r.ConfirmedJoiners) + "\n" +
									string.Join(", ", r.ConfirmedLeavers) + "\n" +
									string.Join(", ", r.ConfirmedViolators)
									;

				Blocks.Items.AddRange(Core.Chain.FindRound((int)Round.Value).Payloads.Select(	(i, j) =>
																									{
																										var li = new ListViewItem(j.ToString());
																										li.Tag = i;
																										li.SubItems.Add(i.Member.ToString());
																										return li;
																									}).ToArray());

				if(Blocks.Items.Count > 0)
				{
					Blocks_ItemSelectionChanged(null, new ListViewItemSelectionChangedEventArgs(Blocks.Items[0], 0, true));

					if(Transactions.Items.Count > 0)
						Transactions_ItemSelectionChanged(null, new ListViewItemSelectionChangedEventArgs(Transactions.Items[0], 0, true));
				}
			}
		}

		private void Blocks_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			Transactions.Items.Clear();
			Operations.Items.Clear();

			if(e.IsSelected)
			{
				lock(Core.Lock)
				{
					Transactions.Items.AddRange((e.Item.Tag as Payload).Transactions.Select((i) =>
																							{
																								var li = new ListViewItem(i.Signer.ToString());
																								li.Tag = i;
																								li.SubItems.Add(i.Id.ToString());
																								li.SubItems.Add(i.Successful ? "OK" : "Failed");
																								return li;
																							}).ToArray());
				}
			}
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
																								var li = new ListViewItem(i.ToString());
																								//li.Tag = i;
																								li.SubItems.Add(i.Result.ToString());
																								return li;
																							}).ToArray());
				}
			}
		}
	}
}
