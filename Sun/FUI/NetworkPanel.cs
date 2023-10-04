using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Uccs.Sun.FUI
{
	public partial class NetworkPanel : MainPanel
	{
		public NetworkPanel(Net.Sun d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			Peers.Items.Clear();
			Members.Items.Clear();
			Funds.Items.Clear();

			lock(Sun.Lock)
			{
				foreach(var i in Sun.Peers.OrderBy(i => i.IP.GetAddressBytes(), new BytesComparer()))
				{
					var r = Peers.Items.Add(i.IP.ToString());
					r.SubItems.Add(i.StatusDescription);
					r.SubItems.Add(i.Retries.ToString());
					r.SubItems.Add(i.PeerRank.ToString());
					r.SubItems.Add(i.BaseRank.ToString());
					r.SubItems.Add(i.ChainRank.ToString());
					r.SubItems.Add(i.LastSeen.ToString(ChainTime.DateFormat.ToString()));
					r.Tag = i;
				}

				foreach(var i in Sun.Mcv.LastConfirmedRound.Members.OrderBy(i => i.Account))
				{
					var li = Members.Items.Add(i.Account.ToString());

					li.SubItems.Add(i.JoinedAt.ToString());
					li.SubItems.Add(Database != null ? Sun.Mcv.Accounts.Find(i.Account, int.MaxValue).Bail.ToHumanString() : null);
					li.SubItems.Add(string.Join(", ", i.BaseRdcIPs.AsEnumerable()));
					li.SubItems.Add(string.Join(", ", i.SeedHubRdcIPs.AsEnumerable()));
				}

				//foreach(var i in Core.Peers.Where(i => i.GetRank(Role.Hub) > 0).OrderByDescending(i => i.GetRank(Role.Hub)).ThenBy(i => i.IP.GetAddressBytes(), new BytesComparer()))
				//{
				//	var li = new ListViewItem(i.IP.ToString());
				//	li.SubItems.Add(i.GetRank(Role.Hub).ToString());
				//	//li.SubItems.Add(i.HubHits.ToString());
				//	Hubs.Items.Add(li);
				//}

				if(Database?.LastConfirmedRound != null)
				{
					foreach(var i in Database.LastConfirmedRound.Funds.OrderBy(i => i))
					{
						var li = new ListViewItem(i.ToString());
						Funds.Items.Add(li);
					}

				}
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
	}
}
