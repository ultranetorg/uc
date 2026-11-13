using Uccs.Net;
using Uccs.Nexus;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public partial class PackagesForm : Form
{
	Nexus	Nexus;
	Flow	Flow;

	public PackagesForm()
	{
		InitializeComponent();

		Results.Items.Clear();
	}

	public PackagesForm(Nexus nexus)
	{
		InitializeComponent();

		Results.Items.Clear();
		Nexus = nexus;
	}

	private void Search_Click(object sender, EventArgs e)
	{
		Results.Items.Clear();

		if(Flow != null)
			Flow.Abort();
	
		Flow = new Flow(5000);

		Search.Enabled = false;

		if(Ura.TryParse(Query.Text, out var a))
		{
			Task.Run(() =>	{
								try
								{
									var r = Nexus.RdnNode.Peering.Call(new ResourceByAddressPpc(a), Flow).Resource;
									
									Invoke(() => { 
												 	var lvi = new ListViewItem(	[
												 									r.Id.ToString(),
												 									a.Domain, 
												 									a.Resource,
												 									null,
												 									null
												 								]);
												 	lvi.Group = Results.Groups[1];
												 	lvi.Tag = r;
												 
												 	Results.Items.Add(lvi);
												 });
								}
								catch(Exception)
								{
								}

								Invoke(() => Search.Enabled = true);
							});
		}
		else
			Search.Enabled = true;

		lock(Nexus.PackageHub)
			foreach(var i in Nexus.PackageHub.Packages.Where(i => i.Address.Domain.Contains(Query.Text, StringComparison.OrdinalIgnoreCase) || 
																  i.Address.Resource.Contains(Query.Text, StringComparison.OrdinalIgnoreCase)))
			{
				var tvi = new ListViewItem(	[
												null,
												i.Address.Domain, 
												i.Address.Resource,
												i.Release.Availability.ToString(),
												null
											]);
				tvi.Group = Results.Groups[2];
				tvi.Tag = i;

				Results.Items.Add(tvi);
			}
	}
}
