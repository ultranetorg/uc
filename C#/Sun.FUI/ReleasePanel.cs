using System;
using System.Linq;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class ReleasePanel : MainPanel
	{
		public ReleasePanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			if(first)
			{
				BindAuthors(Author, () => 
									{ 
										Author.Text = null;
										Author.SelectedIndex = -1;
									});
			}
		}

		private void releases_SelectedIndexChanged(object sender, EventArgs e)
		{
			manifest.Text = null;
		
			if(Releases.SelectedItems.Count > 0)
			{
				var r = Releases.SelectedItems[0].Tag as ReleaseRegistration;
				var m = r.Manifest.ToXon(new XonTextValueSerializator());

				m.Dump(	(n, t) => 
						{
							manifest.Text += new string(' ', t * 3) + n.Name + " = ";
							manifest.Text += (n.Value != null ? n.Serializator.Get<String>(n, n.Value) : null) + "\r\n";
						});
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
	
				foreach(var r in Core.Chain.FindReleases(Author.Text, Product.Text, i => string.IsNullOrWhiteSpace(Platform.Text) || i.Manifest.Address.Platform == Platform.Text))
				{
					var i = new ListViewItem(r.Manifest.Address.ToString());
					
					i.Tag = r;
					//i.SubItems.Add(r.Manifest.Address.ToString());
				
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
				foreach(var p in Chain.Authors.Find(Author.SelectedItem as string, int.MaxValue).Products)
					Product.Items.Add(p);
				
				if(Product.Items.Count > 0)
					Product.SelectedIndex = 0;
			}
		}
	}
}
