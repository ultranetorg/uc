using System.Data;

namespace Uccs.Mcv.FUI;

public partial class NetworkPanel : McvPanel
{
	HomoPeering Peering;

	public NetworkPanel(HomoPeering peering)
	{
		Peering = peering;

		InitializeComponent();
	}

	public override void Open(bool first)
	{
		Reload();
	}

	private void Reload()
	{
		Peers.Items.Clear();

		lock(Peering.Lock)
		{
			foreach(var p in Peering.Peers.OrderByDescending(i => i.Status))
			{
				var r = Peers.Items.Add(p.EP.ToString());
				r.SubItems.Add(p.StatusDescription);
				r.SubItems.Add(p.Retries.ToString());
				r.SubItems.Add(p.PeerRank.ToString());
				r.SubItems.Add(p.LastSeen.ToString(Time.DateFormat.ToString()));
	
				r.SubItems.Add(string.Join(',', Enumerable.Range(0, sizeof(long) * 8).Select(i => 1L << i).Where(i => p.Roles.IsSet(i)).Select(x => $"{x}").ToArray()));
	
				r.Tag = p;
			}
		}
	}

	private void Refresh_Click(object sender, EventArgs e)
	{
		Reload();
	}
}
