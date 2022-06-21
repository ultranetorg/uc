using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class InitialsPanel : MainPanel
	{
		bool loading = false;

		public InitialsPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			manage.Visible = Core.Nas.IsAdministrator;
			nodes.ReadOnly = !Core.Nas.IsAdministrator;

			zone.Items.Clear();

			foreach(var i in  Zone.Names)
			{
				zone.Items.Add(i);
			}

			zone.SelectedItem = Zone.NameByValue(Core.Settings.Zone);

			//ReloadDefaultNodes();
		}

		async private void register_Click(object sender, EventArgs e) 
		{
			try
			{
				manage.Enabled = false;
				nodes.Enabled = false;

				await Core.Nas.SetZone(Zone.ValueByName(zone.SelectedItem as string), nodes.Text, new EthereumFeeForm());

				ReloadDefaultNodes();

			}
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		async private void unregister_Click(object sender, EventArgs e)
		{
			try
			{
				manage.Enabled = false;

				await Core.Nas.RemoveZone(Zone.ValueByName(zone.SelectedItem as string), new EthereumFeeForm());

				ReloadDefaultNodes();

			}
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ReloadDefaultNodes()
		{
			if(!loading)
			{
				loading = true;

				manage.Enabled = false;
				nodes.Enabled = false;
				nodes.Text = "Loading....";
	
				Task.Run(	() =>
							{
								Invoke( (MethodInvoker)delegate()
										{
											nodes.Text = string.Join("\r\n", Core.Nas.GetInitials(Zone.ValueByName(zone.SelectedItem as string)));
						
											manage.Enabled = true;
											nodes.Enabled = true;
											loading = false;
										});
							});

			}
		}

		private void zone_SelectedIndexChanged(object sender, EventArgs e)
		{
			ReloadDefaultNodes();
		}
	}
}
