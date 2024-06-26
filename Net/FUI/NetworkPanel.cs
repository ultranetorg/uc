using System.Data;
using System.Reflection;

namespace Uccs.Net.FUI
{
	public partial class NetworkPanel : MainPanel
	{
		Flow Flow;

		public NetworkPanel(Node d) : base(d)
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

			lock(Node.Lock)
			{
				foreach(var p in Node.Peers.OrderByDescending(i => i.Status))
				{
					var r = Peers.Items.Add(p.IP.ToString());
					r.SubItems.Add(p.StatusDescription);
					r.SubItems.Add(p.Retries.ToString());
					r.SubItems.Add(p.PeerRank.ToString());
					r.SubItems.Add(p.LastSeen.ToString(Time.DateFormat.ToString()));

					r.SubItems.Add(string.Join(',', Enumerable.Range(0, sizeof(long)*8).Select(i => 1L << i).Where(i => p.Roles.IsSet(i)).Select(x => $"{x}").ToArray()));

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
