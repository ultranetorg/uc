using System.Data;
using Org.BouncyCastle.Utilities.Encoders;
//using Uccs.Rdn;

namespace Uccs.Mcv.FUI;

public partial class ChainPanel : McvPanel
{
	McvNode		Node;
	Net.Mcv		Mcv => Node.Mcv;

	public ChainPanel(McvNode node)
	{
		InitializeComponent();

		Node = node;
	}

	public override void Open(bool first)
	{
		if(first)
		{
			lock(Mcv.Lock)
			{
				Mcv.VoteAdded += (b) =>	{
											BeginInvoke((MethodInvoker)delegate
														{
															lock(Mcv.Lock)
															{
																Round.Minimum = Mcv.Settings.Chain != null ? 0 : Mcv.Tail.Last().Id;
																Round.Maximum = Mcv.LastNonEmptyRound.Id;
															}
														});
										};
			}
		}

		lock(Mcv.Lock)
		{
			Round.Minimum = Mcv.Settings.Chain != null ? 0 : Mcv.Tail.Last().Id;
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
																		li.SubItems.Add(i.Nonce.ToString());
																		li.SubItems.Add(i.Round?.Id > 0 ? i.User : null);
																		li.SubItems.Add(i.Operations.Length.ToString());
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

		lock(Mcv.Lock)
		{
			var r = Mcv.FindRound((int)Round.Value);

			InfoValues.Text = (r.Confirmed ? "Confirmed " : "") + "\n" +
								r.ConsensusTime + "\n" +
								(r.Hash != null ? Hex.ToHexString(r.Hash) : null) + "\n"
								//r.TransactionPerByteFee.ToHumanString()
								;

			Votes.Items.AddRange(r.Votes.OrderByDescending(i => i.User)
										.Select((i, j) =>
										{
											var li = new ListViewItem(j.ToString());
											li.Tag = i;
											li.SubItems.Add(i.GetType().Name);
											li.SubItems.Add(i.User.ToString());
											return li;
										}).ToArray());

			var txs = r.Confirmed ? r.ConsensusTransactions : r.OrderedTransactions;

			LoadTransactions(txs);
			LoadOperations(txs.SelectMany(i => i.Operations));

			if(r.ConsensusMemberLeavers != null)
				MemberLeavers.Items.AddRange(r.ConsensusMemberLeavers.Select(i => new ListViewItem(i.ToString())).ToArray());

			if(r.ConsensusViolators != null)
				Violators.Items.AddRange(r.ConsensusViolators.Select(i => new ListViewItem(i.ToString())).ToArray());
		}
	}

	private void Blocks_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		Transactions.Items.Clear();
		Operations.Items.Clear();

		if(e.IsSelected && e.Item.Tag is Vote p)
		{
			lock(Mcv.Lock)
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
			lock(Mcv.Lock)
			{
				LoadOperations((e.Item.Tag as Transaction).Operations);
			}
		}
	}
}
