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
	public partial class GeneratorsPanel : MainPanel
	{
		Font Bold;

		public GeneratorsPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

			Bold = new Font(Font, FontStyle.Bold);
		}

		public override void Open(bool first)
		{
			IPs.Items.Clear();
			Generators.Items.Clear();
			Proxies.Items.Clear();

			if(Core.Database?.LastConfirmedRound != null)
			{
				lock(Core.Lock)
				{
					foreach(var i in Core.Database.LastConfirmedRound.Members.OrderBy(i => i.Account))
					{
						var li = Generators.Items.Add(i.Account.ToString());
	
						if(Core.Settings.Generators.Contains(i.Account))
						{
							li.Font = Bold;
						}
	
						li.Tag = i;
						li.SubItems.Add(i.JoinedAt.ToString());
						li.SubItems.Add(Database != null ? Core.Database.Accounts.Find(i.Account, int.MaxValue).Bail.ToHumanString() : null);
						//li.SubItems.Add(string.Join(", ", i.IPs.AsEnumerable()));
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

		private void Generators_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if(e.IsSelected)
			{
				lock(Core.Lock)
				{
					foreach(var i in (e.Item.Tag as Generator).IPs)
					{
						var li = IPs.Items.Add(i.ToString());
					}

					//foreach(var i in (e.Item.Tag as Member).Proxies)
					{
						var li = Proxies.Items.Add((e.Item.Tag as Generator).Proxy?.ToString());
					}
				}
			}
			else
			{
				IPs.Items.Clear();
				Proxies.Items.Clear();
			}
		}
	}
}
