﻿using System.Drawing;
using System.Windows.Forms;

namespace Uccs.Rdn.FUI
{
	public partial class MembersPanel : MainPanel
	{
		Font Bold;

		public MembersPanel(Mcv mcv) : base(mcv)
		{
			InitializeComponent();

			Bold = new Font(Font, FontStyle.Bold);
		}

		public override void Open(bool first)
		{
			BaseRdcIPs.Items.Clear();
			Generators.Items.Clear();
			Proxies.Items.Clear();

			lock(Node.Lock)
			{
				foreach(var i in Mcv.LastConfirmedRound.Members)
				{
					var li = Generators.Items.Add(i.Account.ToString());
		
					if(Mcv.Settings.Generators.Contains(i.Account))
					{
						li.Font = Bold;
					}
		
					li.Tag = i;
					li.SubItems.Add(i.CastingSince.ToString());
					li.SubItems.Add(i.Bail.ToDecimalString());
					//li.SubItems.Add(string.Join(", ", i.IPs.AsEnumerable()));
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

		private void Generators_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if(e.IsSelected)
			{
				lock(Node.Lock)
				{
					foreach(var i in (e.Item.Tag as Member).BaseRdcIPs)
					{
						var bli = BaseRdcIPs.Items.Add(i.ToString());
					}

					foreach(var i in (e.Item.Tag as Member).SeedHubRdcIPs)
					{
						var li = SeedHubRdcIPs.Items.Add(i.ToString());
					}

					///foreach(var i in (e.Item.Tag as Member).Proxies)
					{
					//	var li = Proxies.Items.Add((e.Item.Tag as MembersResponse.Member).Proxy?.ToString());
					}
				}
			}
			else
			{
				BaseRdcIPs.Items.Clear();
				SeedHubRdcIPs.Items.Clear();
				Proxies.Items.Clear();
			}
		}
	}
}