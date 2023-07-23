using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nethereum.Contracts;
using Org.BouncyCastle.Utilities.Encoders;
using Uccs.Net;

namespace Uccs.Sun.FUI
{
	public partial class ReleasePanel : MainPanel
	{
		Workflow ManifestWorkflow;

		public ReleasePanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			if(first)
			{
			}
		}

		private void search_Click(object sender, EventArgs e)
		{
			try
			{
				Releases.Items.Clear();
				manifest.Text = null;

				if(string.IsNullOrWhiteSpace(Address.Text))
					return;

				var a = ResourceAddress.Parse(Address.Text);

				foreach(var r in Core.Database.Resources.Where(a.Author, i => i.Address.Resource.StartsWith(a.Resource), Core.Database.LastConfirmedRound.Id))
				{
					var i = new ListViewItem(r.Address.ToString());

					i.Tag = r;
					i.SubItems.Add(Hex.ToHexString(r.Data));
					//i.SubItems.Add(r.Channel);

					Releases.Items.Add(i);
				}

				if(Releases.Items.Count == 0)
				{
					var i = new ListViewItem("No results");
					Releases.Items.Add(i);
				}
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}

		private void releases_SelectedIndexChanged(object sender, EventArgs e)
		{
			manifest.Text = null;

			if(Releases.SelectedItems.Count > 0)
			{
				var r = Releases.SelectedItems[0].Tag as ResourceUpdation;

				if(ManifestWorkflow != null)
				{
					ManifestWorkflow.Abort();
				}

				ManifestWorkflow = new Workflow();

				manifest.Text = "Downloading ...";

				Task.Run(() =>	{
									try
									{
										Core.Filebase.GetFile(r.Resource, Package.ManifestFile, ManifestWorkflow);

										var p = Core.PackageBase.Find(new ReleaseAddress(r.Resource));

										BeginInvoke((MethodInvoker)delegate
													{
														manifest.Text = Dump(p.Manifest.ToXon(new XonTextValueSerializator()));
													});
									}
									catch(OperationCanceledException)
									{
									}
								});
			}
		}

		private void all_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Enter)
				search_Click(sender, e);
		}
	}
}
