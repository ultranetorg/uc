using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace Uccs.Net.FUI
{
	public partial class NexusNetworkPanel : MainPanel
	{
		Flow Flow;
 		new NexusNode Node =>  base.Node as NexusNode;

		public NexusNetworkPanel(Node d) : base(d)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			Peers.Items.Clear();

			if(Flow != null && Flow.Active)
			{
				Flow.Abort();
			}

			Flow = Node.Flow.CreateNested(MethodBase.GetCurrentMethod().Name);

			for(int i=1; i<Peers.Columns.Count; i++)
			{
				Peers.Columns.RemoveAt(1);
			}

			lock(Node.Lock)
			{
				foreach(var i in Node.Clusters)
				{
					var c = new ColumnHeader();
					c.Text = i.Net.ToString();
					c.TextAlign = HorizontalAlignment.Center;
					c.Width = 150;
					Peers.Columns.Add(c);
				}

				foreach(var p in Node.Clusters.SelectMany(i => i.Peers))
				{
					var r = Peers.Items.Add(p.IP.ToString());

					foreach(var z in Node.Clusters)
					{
						r.SubItems.Add(z.Peers.Contains(p) ? string.Join(',', Enumerable.Range(0, sizeof(long)*8).Select(i => 1L << i).Where(i => p.Roles.IsSet(i)).Select(x => $"{x}").ToArray()) : "");
					}

					r.Tag = p;
				}
			}

			//Task.Run(() =>	{
			//					FundsResponse rp = null;
			//
			//					try
			//					{
			//						rp = Sun.Call(p => p.GetFunds(), Workflow);
			//					}
			//					catch(OperationCanceledException)
			//					{
			//						return;
			//					}
			//					catch(Exception)
			//					{
			//					}
			//
			//					Invoke(new Action(() =>
			//					{
			//						foreach(var i in rp.Funds.Order())
			//						{
			//							var li = new ListViewItem(i.ToString());
			//							Funds.Items.Add(li);
			//						}
			//					}));
			//				});
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
