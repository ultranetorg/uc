using System.Drawing;
using System.Windows.Forms;
using Uccs.Mcv.FUI;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class RdnMembersPanel : McvPanel
{
	Font Bold;
	McvNode Node;

	public RdnMembersPanel(McvNode node)
	{
		InitializeComponent();

		Node = node;
		Bold = new Font(Font, FontStyle.Bold);
	}

	public override void Open(bool first)
	{
		Refresh_Click(null, EventArgs.Empty);
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
			//lock(Node.Lock)
			{
				foreach(var i in (e.Item.Tag as RdnGenerator).GraphPpiEndpoints)
				{
					var bli = GraphPpiEndpoints.Items.Add(i.ToString());
				}

				foreach(var i in (e.Item.Tag as RdnGenerator).SeedhubPpiEndpoints)
				{
					var li = SeedhubPpiEndpoints.Items.Add(i.ToString());
				}
			}
		}
		else
		{
			GraphPpiEndpoints.Items.Clear();
			SeedhubPpiEndpoints.Items.Clear();
		}
	}

	private void Refresh_Click(object sender, EventArgs e)
	{
		Generators.Items.Clear();
		GraphPpiEndpoints.Items.Clear();
		SeedhubPpiEndpoints.Items.Clear();

		Reload.Enabled = false;
		
		Task.Run(() =>	{ 
							try
							{
								var ms = Node.Peering.Call(new RdnMembersPpc(), new Flow(5000)).Members;

								Invoke(() =>	{ 
													foreach(var i in ms)
													{
														var li = Generators.Items.Add(i.User.ToString());
	
														if(Node.Mcv?.Settings.Generators.Any(g => g.Id == i.User) ?? false)
														{
															li.Font = Bold;
														}
	
														li.Tag = i;
														li.SubItems.Add(i.Since.ToString());
													}
												});
							}
							catch(Exception ex)
							{
							}
							finally
							{
								Invoke(() => Reload.Enabled = true); 
							}
						});

	}
}
