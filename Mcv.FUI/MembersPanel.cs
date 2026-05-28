using System.Drawing;
using System.Windows.Forms;

namespace Uccs.Mcv.FUI;

public partial class MembersPanel : McvPanel
{
	Font		Bold;
	McvNode		Node;
	Net.Mcv		Mcv;

	public MembersPanel(McvNode node)
	{
		InitializeComponent();

		Node = node;
		Bold = new Font(Font, FontStyle.Bold);
	}

	public override void Open(bool first)
	{
		BaseRdcIPs.Items.Clear();
		Generators.Items.Clear();
	
		foreach(var i in Node.Peering.Call(new MembersPpc(), new Flow(5000)).Members)
		{
			var li = Generators.Items.Add(i.User.ToString());
	
			if(Mcv?.Settings.Generators.Any(g => g.Id == i.User) ?? false)
			{
				li.Font = Bold;
			}
	
			li.Tag = i;
			li.SubItems.Add(i.Since.ToString());
			//li.SubItems.Add(i.Pledge.ToString());
			//li.SubItems.Add(string.Join(", ", i.IPs.AsEnumerable()));
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

	private void Generators_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		if(e.IsSelected)
		{
			foreach(var i in (e.Item.Tag as Generator).GraphPpiEndpoints)
			{
				var bli = BaseRdcIPs.Items.Add(i.ToString());
			}
		}
		else
		{
			BaseRdcIPs.Items.Clear();
		}
	}
}
