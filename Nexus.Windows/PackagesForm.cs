using Uccs.Net;
using Uccs.Nexus;

namespace Uccs.Nexus.Windows;

public partial class PackagesForm : Form
{
	public PackagesForm()
	{
		InitializeComponent();
	}

	public PackagesForm(Nexus nexus)
	{
		InitializeComponent();
	}
}
