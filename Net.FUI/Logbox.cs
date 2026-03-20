using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Uccs.Net.FUI;

public partial class Logbox : TextBox, ILogView
{
	Log log;

	public bool ShowSender { get;set; } = false;
	public bool ShowSubject { get;set; } = true;
	public int BufferWidth => MaxLength;

	public Log Log 
	{
		get => log;
		set
		{
			if(Log != null)
			{
				log.Reported -= OnReported;
			}

			log = value;

			if(log != null)
			{
				lock(Log.Messages)
				{
					foreach(var i in Log.Messages)
					{
						OnReported(i);
					}
				}

				log.Reported += OnReported;
			}

		}
	}

	public Logbox()
	{
		InitializeComponent();

		WordWrap = false;
		Font = new Font("Lucida Console", 8);
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		if(log != null)
		{
			log.Reported -= OnReported;
		}

		base.OnHandleDestroyed(e);
	}

	public void OnReported(LogMessage m)
	{
  		var a =	new Action(() =>{
 									StringBuilder t = new ();

									t.Append(' ', 4 * m.Log.Depth);
  
 									if(m.Severity != Uccs.Log.Severity.Info && m.Severity != Uccs.Log.Severity.SubLog)
 									{
										t.Append("!!! ");
										t.Append(m.Severity);
										t.Append(" : ");
									}
 
  									if(ShowSender && m.Sender != null)
 									{	
										t.Append(m.Sender + " : ");
 										t.Append(" : ");
									}
 
 									if(ShowSubject && m.Subject != null)
 									{
 										t.Append(m.Subject); 
 
 										if(m.Text != null)
 											t.Append(" : "); 
 									}
 									
 									if(m.Text != null)
 										t.Append(m.Text[0]);

									t.Append(Environment.NewLine);
 
 									if(m.Text != null)
 									{
  										foreach(var i in m.Text.Skip(1))
  										{
  											t.Append(' ', 4 * m.Log.Depth + 4);
											t.Append(i);
											t.Append(Environment.NewLine);
  										}
 									}

									AppendText(t.ToString());

  									//if(Lines.Length > 100 && Lines.Length > 1100)
  									//{
									//	Lines = Lines.Skip(1000).ToArray();
  									//}
  								});
  
  		if(InvokeRequired)
  			BeginInvoke(a);
  		else
  			a();
	}
}
