using System.Windows.Forms;
using Uccs.Rdn;

namespace Uccs.Net.FUI;

public partial class ResourcesPanel : MainPanel
{
	Flow ManifestWorkflow;
	RdnNode Node;

	public ResourcesPanel(RdnNode mcv)
	{
		Node = mcv;

		InitializeComponent();

		NetworkQuery.KeyDown += (s, e) =>
								{
									if(e.KeyCode == Keys.Enter)
										NetworkSearch_Click(s, e);
								};

		LocalQuery.KeyDown +=	(s, e) =>
								{
									if(e.KeyCode == Keys.Enter)
										LocalSearch_Click(s, e);
								};
	}

	public override void Open(bool first)
	{
		if(first && Node.ResourceHub != null)
		{
			LocalSearch_Click(this, EventArgs.Empty);
		}
	}

	private void NetworkSearch_Click(object sender, EventArgs e)
	{
		if(Node.Mcv == null)
		{
			NetworkReleases.HeaderStyle = ColumnHeaderStyle.None;
			NetworkReleases.Columns.Clear();
			NetworkReleases.Columns.Add(new ColumnHeader() { Width = 500 });
			NetworkReleases.Items.Clear();
			NetworkReleases.Items.Add(new ListViewItem("Base role is not enabled"));
			return;
		}

		try
		{
			NetworkReleases.Items.Clear();

			foreach(var r in Node.Mcv.SearchResources(NetworkQuery.Text))
			{
				var i = new ListViewItem(r.Id.ToString());

				i.Tag = r;

				i.SubItems.Add(r.Address.ToString());
				i.SubItems.Add(string.Join(",", Enum.GetValues<ResourceFlags>().Where(i => i != ResourceFlags.None && ((i & r.Flags) != 0))));
				i.SubItems.Add(r.Data.ToString());
				i.SubItems.Add(r.Outbounds?.Length.ToString());

				NetworkReleases.Items.Add(i);
			}

			if(NetworkReleases.Items.Count == 0)
			{
				var i = new ListViewItem("No results");
				NetworkReleases.Items.Add(i);
			}
		}
		catch(Exception ex)
		{
			ShowError(ex.Message);
		}
	}

	private void LocalSearch_Click(object sender, EventArgs e)
	{
		LocalReleases.Items.Clear();

		foreach(var i in Node.ResourceHub.Resources.Where(i => i.Address.ToString().Contains(LocalQuery.Text)))
		{
			var li = new ListViewItem(i.Address.ToString());
			li.Tag = i;
			li.SubItems.Add(i.Datas.Count.ToString());
			li.SubItems.Add(i.Last?.Type.ToString());
			li.SubItems.Add(i.Last?.Value.ToHex(32));
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
