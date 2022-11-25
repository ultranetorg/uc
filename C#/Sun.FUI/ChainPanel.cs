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

namespace UC.Sun.FUI
{
	public partial class ChainPanel : MainPanel
	{
		public ChainPanel(Core d, Vault vault) : base(d, vault)
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
					Core.Database.BlockAdded += (b) => Round.Maximum = Core.Database.LastNonEmptyRound.Id;

					//Rounds.Items.AddRange(Enumerable.Range(0, Core.Chain.LastNonEmptyRound.Id).OrderByDescending(i => i).Select(i => new ListViewItem(i.ToString())).ToArray());
					Round.Minimum = 0;
					Round.Maximum = Core.Database.LastNonEmptyRound.Id;
					Round.Value = Core.Database.LastNonEmptyRound.Id;
				}
			}
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			Blocks.Items.Clear();
			Transactions.Items.Clear();
			Operations.Items.Clear();
			//Releases.Items.Clear();
			Operation.Text = null;

			lock(Core.Lock)
			{
				var r =  Core.Database.FindRound((int)Round.Value);

				InfoValues.Text =	(r.Confirmed ? "Confirmed " : "") + (r.Voted ? "Voted " : "") + "\n" + 
									r.Time + "\n" + 
									(r.Hash != null ? Hex.ToHexString(r.Hash) : null) + "\n" +
									(r.ConfirmedJoiners != null ? string.Join(", ", r.ConfirmedJoiners) : null) + "\n" +
									(r.ConfirmedLeavers != null ? string.Join(", ", r.ConfirmedLeavers) : null) + "\n" +
									(r.ConfirmedViolators != null ? string.Join(", ", r.ConfirmedViolators) : null)
									;

				Blocks.Items.AddRange(	r.Votes.OrderByDescending(i => i is Payload)
										.Select((i, j) =>
										{
											var li = new ListViewItem(j.ToString());
											li.Tag = i;
											li.SubItems.Add(i.GetType().Name);
											li.SubItems.Add(i.Generator.ToString());
											return li;
										}).ToArray());

				if(Blocks.Items.Count > 0)
				{
					Blocks.Items[0].Selected = true;
				}
			}
		}

		private void Blocks_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			Transactions.Items.Clear();
			Operations.Items.Clear();

			if(e.IsSelected && e.Item.Tag is Payload p)
			{
				lock(Core.Lock)
				{
					Transactions.Items.AddRange(p.Transactions.Select((i) => {
																			 	var li = new ListViewItem(i.Signer.ToString());
																			 	li.Tag = i;
																			 	//li.SubItems.Add(i.Id.ToString());
																			 	li.SubItems.Add(i.SuccessfulOperations.Count().ToString());
																			 	return li;
																			 }).ToArray());
					if(Transactions.Items.Count > 0)
						Transactions.Items[0].Selected = true;
				}
			}
			else
				Transactions.Items.Clear();
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
																								li.Tag = i;
																								li.SubItems.Add(i.ToString());
																								li.SubItems.Add(i.Error);
																								return li;
																							}).ToArray());
					if(Operations.Items.Count > 0)
						Operations.Items[0].Selected = true;
				}
			}
		}

		private void Operations_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			Operation.Text = null;
	
			if(e.IsSelected)
			{
				//if(e.Item.Tag is ReleaseRegistration m)
				//{
				//	m.Manifest.ToXon(new XonTextValueSerializator()).Dump((n, l) => Operation.Text += new string(' ', l * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<String>(n, n.Value))) + Environment.NewLine);
				//}
				//else
					Operation.Text = (e.Item.Tag as Operation).ToString();
			}
		}
	}
}
