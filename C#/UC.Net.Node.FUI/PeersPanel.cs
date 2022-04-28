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
	public partial class PeersPanel : MainPanel
	{
		readonly Timer		Timer = new Timer();

		public PeersPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

			foreach(DataGridViewColumn i in peers.Columns)
			{
				i.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
			}
		}

		public override void Open(bool first)
		{
			peers.Rows.Clear();

			lock(Core.Lock)
			{
				foreach(var i in Core.Peers)
				{
					var r = peers.Rows.Add(new object[]{ i.IP, i.StatusDescription, i.Retries, i.LastSeen.ToString(ChainTime.DateFormat) });
					peers.Rows[r].Tag = i;
				}
			}

			peers.Sort(peers.Columns[0], ListSortDirection.Ascending);

			Timer.Tick += RefreshInfo;
			Timer.Interval = 500;
			Timer.Start();
		}

		public override void Close()
		{
			Timer.Stop();
			Timer.Tick -= RefreshInfo;
		}

		void RefreshInfo(object myObject, EventArgs myEventArgs)
		{ 
			if(peers.Rows.Count >= 0)
			{
				var rows = peers.Rows.Cast<DataGridViewRow>();

				lock(Core.Lock)
				{
					foreach(var i in Core.Peers)
					{
						var r = rows.FirstOrDefault(j => j.Tag as Peer == i);
					
						if(r != null)
						{
							r.Cells[1].Value = i.StatusDescription;
							r.Cells[2].Value = i.Retries;
							r.Cells[3].Value = i.LastSeen.ToString(ChainTime.DateFormat);
						}
					}
				}
			}
		}
	}
}
