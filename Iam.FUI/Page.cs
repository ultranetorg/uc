namespace Uccs.Iam.FUI;

public class Page : UserControl
{
	public bool First = true;

	public virtual void Open(bool first){ }
	public virtual void Close(){ }
	public virtual void PeriodicalRefresh(){ }

	public Page()
	{
	}
}
