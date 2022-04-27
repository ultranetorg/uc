using System;
using System.Linq;

namespace UC
{
	public class ConsoleLogView
	{
		Log Log;
		public bool ShowSender { get;set; } = false;
		public bool ShowSubject { get;set; } = false;

		public ConsoleLogView(Log log, bool showsender, bool showsubject)
		{
			ShowSender = showsender;
			ShowSubject = showsubject;

			Log = log;

			lock(Log.Messages)
			{
				foreach(var i in Log.Messages)
				{
					OnReported(i);
				}
			}
	
			log.Reported += OnReported;
		}


		public void OnReported(LogMessage m)
		{
			var prev = Console.ForegroundColor;

			Console.ForegroundColor = m.Severity switch
												 { 
													Log.Severity.Error => ConsoleColor.Red,
													Log.Severity.Warning => ConsoleColor.Yellow,
													_ => prev
												 };

			Console.Write(new string(' ', 4)); 

 			if(ShowSender && m.Sender != null)
				Console.Write(m.Sender + " : ");

			if(ShowSubject && m.Subject != null)
			{
				Console.Write(m.Subject); 

				if(m.Text != null)
					Console.Write(" : "); 
			}

			if(m.Text != null)
			{
				Console.Write(m.Text[0]);
			}

			Console.WriteLine();

			if(m.Text != null)
			{
				foreach(var t in m.Text.Skip(1))
				{
					var i = 0;
																	
					while(i < t.Length)
					{
						var w = Console.WindowWidth > 4 ? Math.Min(t.Length - i, Console.WindowWidth - 4) : t.Length;
						Console.Write(new string(' ', 8) + t.Substring(i, w));
						i += w;
					}
				
					Console.WriteLine();
				}
			}
				
			Console.ForegroundColor = prev;
		}
	}
}
