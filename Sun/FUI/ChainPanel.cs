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
		public ChainPanel(Mcv mcv) : base(mcv)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			if(first)
			{
				lock(Sun.Lock)
				{
					Mcv.VoteAdded += (b) =>	{
												BeginInvoke((MethodInvoker)delegate
															{
																lock(Sun.Lock)
																{
																	Round.Minimum = Mcv.Roles.HasFlag(Role.Chain) ? 0 : Mcv.Tail.Last().Id;
																	Round.Maximum = Mcv.LastNonEmptyRound.Id;
																}
															});
											};
				}
			}

			lock(Sun.Lock)
			{
				Round.Minimum = Mcv.Roles.HasFlag(Role.Chain) ? 0 : Mcv.Tail.Last().Id;
				Round.Maximum = Mcv.Tail.First().Id;
				Round.Value = Mcv.LastNonEmptyRound.Id;
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
																			li.SubItems.Add(i.Id.ToString());
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
																		li.SubItems.Add(i.Id.ToString());
																		li.SubItems.Add(i.ToString());
																		return li;
																	}).ToArray());
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			Votes.Items.Clear();
			Transactions.Items.Clear();
			Operations.Items.Clear();
			MemberJoiners.Items.Clear();
			MemberLeavers.Items.Clear();
			AnalyzerJoiners.Items.Clear();
			AnalyzerLeavers.Items.Clear();
			FundJoiners.Items.Clear();
			FundLeavers.Items.Clear();
			Emissions.Items.Clear();
			Migrations.Items.Clear();

			lock(Sun.Lock)
			{
				var r = Mcv.FindRound((int)Round.Value);

				InfoValues.Text = (r.Confirmed ? "Confirmed " : "") + "\n" +
									r.ConsensusTime + "\n" +
									(r.Hash != null ? Hex.ToHexString(r.Hash) : null) + "\n"
									//r.TransactionPerByteFee.ToHumanString()
									;

				Votes.Items.AddRange(r.Votes.OrderByDescending(i => i.Generator)
											.Select((i, j) =>
											{
												var li = new ListViewItem(j.ToString());
												li.Tag = i;
												li.SubItems.Add(i.GetType().Name);
												li.SubItems.Add(i.Generator.ToString());
												return li;
											}).ToArray());

				var txs = r.Confirmed ? r.ConsensusTransactions : r.OrderedTransactions;
				LoadTransactions(txs);
				LoadOperations(txs.SelectMany(i => i.Operations));

				if(r.Id > 0)
					MemberJoiners.Items.AddRange(r.Members.Where(i => !r.Previous.Members.Any(j => i.Account == j.Account)).Select(i => new ListViewItem(i.ToString())).ToArray());
				MemberLeavers.Items.AddRange(r.ConsensusMemberLeavers.Select(i => new ListViewItem(i.ToString())).ToArray());
				Violators.Items.AddRange(r.ConsensusViolators.Select(i => new ListViewItem(i.ToString())).ToArray());

				FundJoiners.Items.AddRange(r.ConsensusFundJoiners.Select(i => new ListViewItem(i.ToString())).ToArray());
				FundLeavers.Items.AddRange(r.ConsensusFundLeavers.Select(i => new ListViewItem(i.ToString())).ToArray());

				Emissions.Items.AddRange((r as RdsRound)?.ConsensusEmissions.Select(i => new ListViewItem(i.ToString())).ToArray());
				
				if(r is RdsRound rr)
				{
					Migrations.Items.AddRange(rr.Migrations.Select(i => new ListViewItem(i.ToString())).ToArray());
				}
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
