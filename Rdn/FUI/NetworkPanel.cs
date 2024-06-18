using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Uccs.Rdn.FUI
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
				var mids = Node.Peers.SelectMany(p => p.Zones).DistinctBy(i => i.Key).Select(i => i.Key);
	
				foreach(var i in mids)
				{
					var c = new ColumnHeader();
					c.Text = i.ToString();
					c.TextAlign = HorizontalAlignment.Center;
					c.Width = 150;
					Peers.Columns.Add(c);
				}

				foreach(var i in Node.Peers.OrderByDescending(i => i.Status))
				{
					var r = Peers.Items.Add(i.IP.ToString());
					r.SubItems.Add(i.StatusDescription);
					r.SubItems.Add(i.Retries.ToString());
					r.SubItems.Add(i.PeerRank.ToString());
					r.SubItems.Add(i.LastSeen.ToString(Time.DateFormat.ToString()));

					foreach(var j in mids)
					{
						r.SubItems.Add(i.Zones.TryGetValue(j, out var ranks) ? string.Join(',', Enumerable.Range(0, sizeof(long)*8).Select(i => 1L << i).Where(i => ranks.IsSet(i)).Select(x => $"{i}").ToArray()) : "");
					}

					r.Tag = i;
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
