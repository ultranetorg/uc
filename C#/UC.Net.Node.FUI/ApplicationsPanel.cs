using Nethereum.Hex.HexConvertors.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class ApplicationsPanel : MainPanel
	{
		public ApplicationsPanel(Dispatcher d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		private void search_Click(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(name.Text))
				return;

			results.Items.Clear();
/*			
			foreach(var r in Dispatcher.Database.FindProducts(i => i.Product == name.Text && i. OS.Platform.ToString()))
			{
				var i = new ListViewItem(r.Manifest.Product);
				
				i.Tag = r;
				//i.SubItems.Add(r.Author);
				//i.SubItems.Add();
				//i.SubItems.Add();
			
				results.Items.Add(i);
			}*/

			if(results.Items.Count == 0)
			{
				var i = new ListViewItem("No results");
			
				results.Items.Add(i);
			}
		}

		private void name_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Enter)
				search_Click(sender, e);
		}
	}
}
