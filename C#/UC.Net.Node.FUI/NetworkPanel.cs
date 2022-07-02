using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class NetworkPanel : MainPanel
	{
		public NetworkPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

// 			foreach(DataGridViewColumn i in peers.Columns)
// 			{
// 				i.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
// 			}
		}

		public override void Open(bool first)
		{
			Peers.Items.Clear();
			Generators.Items.Clear();
			Funds.Items.Clear();
			Hubs.Items.Clear();

			lock(Core.Lock)
			{
				foreach(var i in Core.Peers.OrderBy(i => i.IP.GetAddressBytes(), new BytesComparer()))
				{
					var r = Peers.Items.Add(i.IP.ToString());
					r.SubItems.Add(i.StatusDescription);
					r.SubItems.Add(i.Retries.ToString());
					r.SubItems.Add(string.Join(", ", Enum.GetValues<Role>().Where(j => i.Role.HasFlag(j))));
					r.SubItems.Add(i.LastSeen.ToString(ChainTime.DateFormat.ToString()));
					r.Tag = i;
				}

				foreach(var i in (Chain != null ? Chain.Members : Core.Members).OrderBy(i => i.JoinedGeneratorsAt))
				{
					var li = Generators.Items.Add(i.Generator.ToString());

					li.SubItems.Add(i.JoinedGeneratorsAt.ToString());
					li.SubItems.Add(Chain != null ? Core.Chain.Accounts.FindLastOperation<CandidacyDeclaration>(i.Generator).Bail.ToHumanString() : null);
					li.SubItems.Add(i.IP.ToString());
				}

				foreach(var i in Core.Peers.Where(i => i.Role.HasFlag(Role.Hub)).OrderBy(i => i.IP.GetAddressBytes(), new BytesComparer()))
				{
					var li = new ListViewItem(i.IP.ToString());
					li.SubItems.Add(i.HubHits.ToString());
					Hubs.Items.Add(li);
				}

				if(Chain != null)
				{
					foreach(var i in Chain.Funds.OrderBy(i => i))
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
