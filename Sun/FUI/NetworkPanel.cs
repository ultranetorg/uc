using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace Uccs.Sun.FUI
{
	public partial class NetworkPanel : MainPanel
	{
		Flow Flow;

		public NetworkPanel(Net.Sun d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			Peers.Items.Clear();
			Members.Items.Clear();

			if(Flow != null && Flow.Active)
			{
				Flow.Abort();
			}

			Flow = Sun.Flow.CreateNested(MethodBase.GetCurrentMethod().Name);

			lock(Sun.Lock)
			{
				foreach(var i in Sun.Peers.OrderByDescending(i => i.Status))
				{
					var r = Peers.Items.Add(i.IP.ToString());
					r.SubItems.Add(i.StatusDescription);
					r.SubItems.Add(i.Retries.ToString());
					r.SubItems.Add(i.PeerRank.ToString());
					r.SubItems.Add(i.BaseRank.ToString());
					r.SubItems.Add(i.ChainRank.ToString());
					r.SubItems.Add(i.LastSeen.ToString(Time.DateFormat.ToString()));
					r.Tag = i;
				}
			}

			Task.Run(() =>
			{
				MembersResponse rp = null;

				try
				{
					rp = Sun.Call(p => p.Request(new MembersRequest()), Flow);
				}
				catch(OperationCanceledException)
				{
					return;
				}
				catch(Exception)
				{
				}

				Invoke(new Action(() =>	{
											foreach(var i in rp.Members.OrderBy(i => i.Account))
											{
												var li = Members.Items.Add(i.Account.ToString());

												li.SubItems.Add("no data");
												li.SubItems.Add("no data");
												li.SubItems.Add(string.Join(", ", i.BaseRdcIPs.AsEnumerable()));
												li.SubItems.Add(string.Join(", ", i.SeedHubRdcIPs.AsEnumerable()));
											}
										}));
			});


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
