using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using UC;
using System.Threading.Tasks;

namespace UC.Net.Node
{
	public partial class Logbox : TextBox
	{
		Log log;

		public bool ShowSender { get;set; } = false;
		public bool ShowSubject { get;set; } = true;

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
 			var a =	new Action( () =>
 								{
 									if(Lines.Length > 1000)
 									{
 										int p = 0;
 
 										for(int i = 0; i < Lines.Length - 1000; i++)
 										{
 											 p = Text.IndexOf(Environment.NewLine, p);
 										}
 
 										Text = Text.Remove(0, p + Environment.NewLine.Length);
 									}
 
									if(m.Severity != UC.Log.Severity.Info)
										AppendText("!!! " + m.Severity + " : ");

 									if(ShowSender && m.Sender != null)
										AppendText(m.Sender + " : ");

									if(ShowSubject && m.Subject != null)
									{
										AppendText(m.Subject); 

										if(m.Text != null)
											AppendText(" : "); 
									}

									if(m.Text != null)
									{
										AppendText(m.Text[0]);
									}

									AppendText(Environment.NewLine);

									if(m.Text != null)
									{
 										foreach(var i in m.Text.Skip(1))
 										{
 											AppendText(new string(' ', 4) + i + Environment.NewLine);
 										}
									 }

 								});
 
 			if(InvokeRequired)
 				BeginInvoke(a);
 			else
 				a();
		}
	}
}
