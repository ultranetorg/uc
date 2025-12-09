using Uccs.Net;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

public class Page : UserControl
{
	public bool First = true;
	public Nexus Nexus;

	public virtual void Open(bool first){ }
	public virtual void Close(){ }
	public virtual void PeriodicalRefresh(){ }

	public Page()
	{
	}

	public Page(Nexus nexus) : this()
	{
		Nexus = nexus;
	}

	public void ShowException(string message, Exception ex)
	{
		MessageBox.Show(ParentForm, message + " (" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	public void ShowError(string message)
	{
		MessageBox.Show(ParentForm, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
}
