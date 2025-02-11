using System.Windows.Forms;
using Uccs.Rdn;

namespace Uccs.Net.FUI;

public partial class HubPanel : MainPanel
{
	RdnNode Node;

	public HubPanel(RdnNode node)
	{
		InitializeComponent();

		Node = node;
		// 			foreach(DataGridViewColumn i in peers.Columns)
		// 			{
		// 				i.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
		// 			}
	}

	public override void Open(bool first)
	{
		if(first)
		{
			Search_Click(this, null);
		}
	}

	public override void PeriodicalRefresh()
	{
		//if(peers.Rows.Count >= 0)
		//{
		//	var rows = peers.Rows.Cast<DataGridViewRow>();
		//
		//	lock(Core.Lock)
		//	{
		//		foreach(var i in Core.Peers)
		//		{
		//			var r = rows.FirstOrDefault(j => j.Tag as Peer == i);
		//		
		//			if(r != null)
		//			{
		//				r.Cells[1].Value = i.StatusDescription;
		//				r.Cells[2].Value = i.Retries;
		//				r.Cells[3].Value = i.LastSeen.ToString(ChainTime.DateFormat);
		//			}
		//		}
		//	}
		//}
	}

	private void Search_Click(object sender, EventArgs e)
	{
		Packages.Items.Clear();
		Seeds.Items.Clear();

		lock(Node.SeedHub.Lock)
		{
			foreach(var i in Node.SeedHub.Releases.Take(1000))
			{
				var r = Packages.Items.Add(i.Key.ToString());
				//r.SubItems.Add(i.Key.Resource);
				//r.SubItems.Add(i.Key.Realization.Name.ToString());
				//r.SubItems.Add(i.Key.Version.ToString());
				//r.SubItems.Add(i.Key.Distributives.ToString());
				r.Tag = i.Value;
			}
		}
	}

	private void Packages_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		if(e.IsSelected)
		{
			Seeds.Items.Clear();

			lock(Node.SeedHub.Lock)
			{
				foreach(var i in e.Item.Tag as List<Seed>)
				{
					var r = Seeds.Items.Add(i.IP.ToString());
					r.SubItems.Add(i.Availability.ToString());
					r.SubItems.Add(i.Arrived.ToString(Time.DateFormat));
				}
			}
		}
	}
}
