using System.Windows.Forms;

namespace Uccs.Sun.FUI
{
	public partial class ResourcesPanel : MainPanel
	{
		Workflow ManifestWorkflow;

		public ResourcesPanel(Net.Sun d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

			NetworkQuery.KeyDown += (s, e) =>
									{
										if(e.KeyCode == Keys.Enter)
											NetworkSearch_Click(s, e);
									};

			LocalQuery.KeyDown += (s, e) =>
									{
										if(e.KeyCode == Keys.Enter)
											LocalSearch_Click(s, e);
									};
		}

		public override void Open(bool first)
		{
			if(first)
			{
				LocalSearch_Click(this, EventArgs.Empty);
			}
		}

		private void NetworkSearch_Click(object sender, EventArgs e)
		{
			if(!Sun.Roles.HasFlag(Role.Base))
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

				foreach(var r in Sun.Mcv.QueryResource(NetworkQuery.Text))
				{
					var i = new ListViewItem(r.Id.ToString());

					i.Tag = r;

					i.SubItems.Add(r.Address.ToString());
					i.SubItems.Add(string.Join(",", Enum.GetValues<ResourceFlags>().Where(i => i != ResourceFlags.None && i != ResourceFlags.Unchangables && ((i & r.Flags) != 0))));
					i.SubItems.Add(r.Data?.ToHex(32));
					i.SubItems.Add(r.Resources.Length.ToString());

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
			if(!Sun.Roles.HasFlag(Role.Seed))
			{
				LocalReleases.HeaderStyle = ColumnHeaderStyle.None;
				LocalReleases.Columns.Clear();
				LocalReleases.Columns.Add(new ColumnHeader() { Width = 500 });
				LocalReleases.Items.Clear();
				LocalReleases.Items.Add(new ListViewItem("Seed role is not enabled"));
				return;
			}

			LocalReleases.Items.Clear();

			foreach(var i in Sun.ResourceHub.Resources.Where(i => i.Address.ToString().Contains(LocalQuery.Text)))
			{
				var li = new ListViewItem(i.Address.ToString());
				li.Tag = i;
				li.SubItems.Add(i.Datas.Count.ToString());
				li.SubItems.Add(i.Last.Data.ToHex(128));

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
}
