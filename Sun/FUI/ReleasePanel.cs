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
	
				if(string.IsNullOrWhiteSpace(Author.Text))
					return;
	
				foreach(var r in Core.Database.Releases.Where(Author.Text, Product.Text, i => string.IsNullOrWhiteSpace(Platform.Text) || i.Address.Realization == Platform.Text, Core.Database.LastConfirmedRound.Id))
				{
					var i = new ListViewItem(r.Address.ToString());
					
					i.Tag = r;
					i.SubItems.Add(Hex.ToHexString(r.Manifest));
					i.SubItems.Add(r.Channel);
				
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
				var r = Releases.SelectedItems[0].Tag as ReleaseRegistration;

				if(ManifestWorkflow != null)
				{
					ManifestWorkflow.Abort();
				}

				ManifestWorkflow = new Workflow();

				manifest.Text = "Downloading ...";

				Task.Run(() =>	{ 
									try
									{
										var m = Core.Call(	Role.Seed, 
															p =>{
																	var m = p.GetManifest(r.Release).Manifests.FirstOrDefault();
																	
																	if(m == null)
																		throw new RdcException(Uccs.Net.RdcError.Null);

																	return m;
																},
															ManifestWorkflow);

										BeginInvoke((MethodInvoker)delegate
													{
														manifest.Text = Dump(m.ToXon(new XonTextValueSerializator()));
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

		private void Author_SelectedIndexChanged(object sender, EventArgs e)
		{
			Product.Text = null;
			Product.Items.Clear();

			if(Author.SelectedItem != null)
			{
				/// TODO: too slow
				foreach(var p in Database.Products.Where(i => i.Address.Author == Author.SelectedItem as string).Select(i => i.Address.Product))
					Product.Items.Add(p);
				
				if(Product.Items.Count > 0)
					Product.SelectedIndex = 0;
			}
		}
	}
}
