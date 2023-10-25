using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Sun.FUI
{
	public partial class ChainPanel : MainPanel
	{
		public ChainPanel(Net.Sun d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			if(first)
			{
				lock(Sun.Lock)
				{
					Sun.Mcv.VoteAdded += (b) =>
					{
						BeginInvoke((MethodInvoker)delegate
									{
										lock(Sun.Lock)
										{
											Round.Minimum = Sun.Roles.HasFlag(Role.Chain) ? 0 : Sun.Mcv.Tail.Last().Id;
											Round.Maximum = Sun.Mcv.LastNonEmptyRound.Id;
										}
									});
					};

					//Rounds.Items.AddRange(Enumerable.Range(0, Core.Chain.LastNonEmptyRound.Id).OrderByDescending(i => i).Select(i => new ListViewItem(i.ToString())).ToArray());
					Round.Minimum = Sun.Roles.HasFlag(Role.Chain) ? 0 : Sun.Mcv.Tail.Last().Id;
					Round.Maximum = Sun.Mcv.LastNonEmptyRound.Id;
					Round.Value = Sun.Mcv.LastNonEmptyRound.Id;
				}
			}
		}

		public override void Close()
		{
		}

		void LoadTransactions(IEnumerable<Transaction> transactions)
		{
			Transactions.Items.AddRange(transactions.Select((i, j) =>	{
																			var li = new ListViewItem(j.ToString());
																			li.Tag = i;
																			li.SubItems.Add(new BigInteger(i.Id.Serial, true, true).ToString());
																			li.SubItems.Add(i.Nid.ToString());
																			li.SubItems.Add(i.Signer.ToString());
																			li.SubItems.Add(i.Operations.Length.ToString());
																			li.SubItems.Add(i.Fee.ToString());
																			return li;
																		}).ToArray());
		}

		void LoadOperations(IEnumerable<Operation> operations)
		{
			Operations.Items.AddRange(operations.Select((i, j) =>	{
																		var li = new ListViewItem(j.ToString());
																		li.Tag = i;
																		li.SubItems.Add(new BigInteger(i.Id.Serial, true, true).ToString());
																		li.SubItems.Add(i.ToString());
																		return li;
																	}).ToArray());
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			Votes.Items.Clear();
			Transactions.Items.Clear();
			Operations.Items.Clear();
			Joiners.Items.Clear();

			lock(Sun.Lock)
			{
				var r = Sun.Mcv.FindRound((int)Round.Value);

				InfoValues.Text = (r.Confirmed ? "Confirmed " : "") + (r.Voted ? "Voted " : "") + "\n" +
									r.ConfirmedTime + "\n" +
									(r.Hash != null ? Hex.ToHexString(r.Hash) : null) + "\n"
									//r.TransactionPerByteFee.ToHumanString()
									;

				Votes.Items.AddRange(r.Votes.OrderByDescending(i => i.Transactions.Any())
											.Select((i, j) =>
											{
												var li = new ListViewItem(j.ToString());
												li.Tag = i;
												li.SubItems.Add(i.GetType().Name);
												li.SubItems.Add(i.Generator.ToString());
												return li;
											}).ToArray());

				if(r.Id > 0)
					Joiners.Items.AddRange(r.Members.Where(i => !r.Previous.Members.Any(j => i.Account == j.Account)).Select(i => new ListViewItem(i.ToString())).ToArray());
				Leavers.Items.AddRange(r.ConfirmedMemberLeavers.Select(i => new ListViewItem(i.ToString())).ToArray());
				Violators.Items.AddRange(r.ConfirmedViolators.Select(i => new ListViewItem(i.ToString())).ToArray());


				var txs = r.Confirmed ? r.ConfirmedTransactions : r.OrderedTransactions;
				LoadTransactions(txs);
				LoadOperations(txs.SelectMany(i => i.Operations));
			}
		}

		private void Blocks_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			Transactions.Items.Clear();
			Operations.Items.Clear();

			if(e.IsSelected && e.Item.Tag is Vote p)
			{
				lock(Sun.Lock)
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
				lock(Sun.Lock)
				{
					LoadOperations((e.Item.Tag as Transaction).Operations);
				}
			}
		}
	}
}
