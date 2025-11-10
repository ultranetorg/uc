namespace Uccs.Nexus.Windows;

public class Page : UserControl
{
	public bool First = true;

	public virtual void Open(bool first){ }
	public virtual void Close(){ }
	public virtual void PeriodicalRefresh(){ }

	public Page()
	{
	}

	public void ShowException(string message, Exception ex)
	{
		MessageBox.Show(this, message + " (" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	public void ShowError(string message)
	{
		MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
}
