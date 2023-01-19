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
		}

		public override void Open(bool first)
		{
			if(first)
			{
				lock(Core.Lock)
				{
					Core.Database.BlockAdded += (b) =>
												{
													BeginInvoke((MethodInvoker)delegate
																{ 
																	lock(Core.Lock)
																	{
																		Round.Minimum = Core.Settings.Database.Chain ? 0 : Core.Database.Rounds.Last().Id;
																		Round.Maximum = Core.Database.LastNonEmptyRound.Id; 
																	}
																});
												};

					//Rounds.Items.AddRange(Enumerable.Range(0, Core.Chain.LastNonEmptyRound.Id).OrderByDescending(i => i).Select(i => new ListViewItem(i.ToString())).ToArray());
					Round.Minimum = Core.Settings.Database.Chain ? 0 : Core.Database.Rounds.Last().Id;
					Round.Maximum = Core.Database.LastNonEmptyRound.Id;
					Round.Value = Core.Database.LastNonEmptyRound.Id;
				}
			}
		}

		public override void Close()
		{
		}

		void LoadTransactions(IEnumerable<Transaction> transactions)
		{
			Transactions.Items.AddRange(transactions.Select((i) => {
																		var li = new ListViewItem(i.Signer.ToString());
																		li.Tag = i;
																		//li.SubItems.Add(i.Id.ToString());
																		li.SubItems.Add(i.SuccessfulOperations.Count().ToString());
																		return li;
																	}).ToArray());
		}

		void LoadOperations(IEnumerable<Operation> operations)
		{
			Operations.Items.AddRange(operations.Select((i) =>	{
																	var li = new ListViewItem(i.Id.ToString());
																	li.Tag = i;
																	li.SubItems.Add(i.ToString());
																	li.SubItems.Add(i.Error);
																	return li;
																}).ToArray());
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			Blocks.Items.Clear();
			Transactions.Items.Clear();
			Operations.Items.Clear();

			lock(Core.Lock)
			{
				var r =  Core.Database.FindRound((int)Round.Value);

				InfoValues.Text =	(r.Confirmed ? "Confirmed " : "") + (r.Voted ? "Voted " : "") + "\n" + 
									r.Time + "\n" + 
									(r.Hash != null ? Hex.ToHexString(r.Hash) : null) + "\n" +
									string.Join(", ", r.ConfirmedJoiners) + "\n" +
									string.Join(", ", r.ConfirmedLeavers) + "\n" +
									string.Join(", ", r.ConfirmedViolators)
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

				LoadTransactions(r.Payloads.SelectMany(i => i.Transactions));
				LoadOperations(r.Payloads.SelectMany(i => i.Transactions.SelectMany(i => i.Operations)));
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
					LoadTransactions(p.Transactions);
					LoadOperations(p.Transactions.SelectMany(i => i.Operations));
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
					LoadOperations((e.Item.Tag as Transaction).Operations);
				}
			}
		}
	}
}
