using System.Windows.Forms;
using Uccs.Mcv.FUI;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class ResourcesPanel : McvPanel
{
	Flow		ManifestWorkflow;
	RdnNode		Node;

	public ResourcesPanel(RdnNode node)
	{
		Node = node;

		InitializeComponent();

		OnlineQuery.KeyDown += (s, e) =>
								{
									if(e.KeyCode == Keys.Enter)
										OnlineSearch_Click(s, e);
								};

		LocalQuery.KeyDown +=	(s, e) =>
								{
									if(e.KeyCode == Keys.Enter)
										LocalSearch_Click(s, e);
								};
	}

	public override void Open(bool first)
	{
		//if(first && Node.ResourceHub != null)
		//{
		//	LocalSearch_Click(this, EventArgs.Empty);
		//}

		if(first)
		{
			LocalQuery.Enabled = Node.ResourceHub != null;
			LocalReleases.Enabled = Node.ResourceHub != null;
			LocalSearch.Enabled = Node.ResourceHub != null;
		}
	}

	private void OnlineSearch_Click(object sender, EventArgs e)
	{
		//if(Node.Mcv == null)
		//{
		//	NetworkReleases.HeaderStyle = ColumnHeaderStyle.None;
		//	NetworkReleases.Columns.Clear();
		//	NetworkReleases.Columns.Add(new ColumnHeader() {Width = 500});
		//	NetworkReleases.Items.Clear();
		//	NetworkReleases.Items.Add(new ListViewItem("Base role is not enabled"));
		//	return;
		//}
	
		try
		{
			NetworkReleases.Items.Clear();
	
			if(!string.IsNullOrWhiteSpace(OnlineQuery.Text))
			{
				Ura.Parse(OnlineQuery.Text, out var d, out var r);

				var domain = Node.Peering.Call(new DomainPpc(d), new Flow(5000));
	
				foreach(var i in Node.Peering.Call(new QueryResourcePpc {Domain = domain.Domain.Id, Query = r}, new Flow(5000)).Resources)
				{
					var li = new ListViewItem(i.Id.ToString());
		
					li.Tag = r;
		
					li.SubItems.Add(i.Name);
					li.SubItems.Add(i.Flags.ToString());
					li.SubItems.Add(i.Data?.Type.Control.ToString());
					li.SubItems.Add(i.Data?.Type.Content.ToString());
					li.SubItems.Add(i.Data?.Value.Length.ToString());
					li.SubItems.Add(i.Inbounds?.Length.ToString());
					li.SubItems.Add(i.Outbounds?.Length.ToString());
		
					NetworkReleases.Items.Add(li);
				}
			}
	
// 			if(NetworkReleases.Items.Count == 0)
// 			{
// 				var i = new ListViewItem("No results");
// 				NetworkReleases.Items.Add(i);
// 			}
		}
		catch(Exception ex)
		{
			ShowError(ex.Message);
		}
	}

	private void LocalSearch_Click(object sender, EventArgs e)
	{
		LocalReleases.Items.Clear();
	
		foreach(var i in Node.ResourceHub.Resources.Where(i => i.Address.ToString().Contains(LocalQuery.Text, StringComparison.InvariantCultureIgnoreCase)))
		{
			var li = new ListViewItem(i.Id?.ToString());
			li.Tag = i;
			li.SubItems.Add(i.Address.ToString());
			li.SubItems.Add(i.Last?.Type.Control.ToString());
			li.SubItems.Add(i.Last?.Type.Content.ToString());
			li.SubItems.Add(i.Last?.Value.Length.ToString());
	
			LocalReleases.Items.Add(li);
		}
	}

	// 		private void releases_SelectedIndexChanged(object sender, EventArgs e)
	// 		{
	// 			if(Releases.SelectedItems.Count > 0)
	// 			{
	// 				var r = Releases.SelectedItems[0].Tag as Resource;
	// 
	// 				if(ManifestWorkflow != null)
	// 				{
	// 					ManifestWorkflow.Abort();
	// 				}
	// 
	// 				ManifestWorkflow = new Workflow("releases_SelectedIndexChanged");
	// 
	// 				//manifest.Text = "Downloading ...";
	// 
	// 				Task.Run(() =>
	// 				{
	// 					try
	// 					{
	// 						///Sun.Resources.GetFile(r.Address, r.Data, Package.ManifestFile, null, null, ManifestWorkflow);
	// 
	// 						var p = Sun.Packages.Find(new PackageAddress(r.Address));
	// 
	// 						BeginInvoke((MethodInvoker)delegate
	// 									{
	// 										manifest.Text = Dump(p.Manifest.ToXon(new XonTextValueSerializator()));
	// 									});
	// 					}
	// 					catch(OperationCanceledException)
	// 					{
	// 					}
	// 				});
	// 			}
	// 		}
}
