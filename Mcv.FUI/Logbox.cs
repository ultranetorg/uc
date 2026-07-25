using System.Collections.Concurrent;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Uccs.Mcv.FUI;

public partial class Logbox : TextBox, ILogView
{
	public bool						ShowSender { get;set; } = false;
	public bool						ShowSubject { get;set; } = true;
	public int						BufferWidth => MaxLength;
	ConcurrentQueue<LogMessage>		Messages;
	Log								_Log;

	public Log Log 
	{
		get => _Log;
		set
		{
			if(Log != null)
			{
				_Log.RemoveListener(Messages);
				_Log.Reported -= OnReported;
			}

			_Log = value;

			if(_Log != null)
			{
				Messages = _Log.AddListener();

				foreach(var i in Messages)
					OnReported(i);

				_Log.Reported += OnReported;
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
		if(Log != null)
		{
			Log = null;
		}

		base.OnHandleDestroyed(e);
	}

	public void OnReported(LogMessage message)
	{
	 	StringBuilder t = new ();

 		while(Messages.TryDequeue(out var m))
 		{
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
	  				t.Append(' ', 4);
					t.Append(i);
					t.Append(Environment.NewLine);
	  			}
	 		}
 		}
  
  		if(InvokeRequired)
  			BeginInvoke(() => AppendText(t.ToString()));
  		else
  			AppendText(t.ToString());
	}
}
