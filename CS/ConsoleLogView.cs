﻿namespace Uccs;

public interface ILogView
{
	public abstract bool ShowSender { get;set; }
	public abstract bool ShowSubject { get;set; }
	public abstract int BufferWidth { get; }
}

public class ConsoleLogView : ILogView
{
	public Log		Log { get; protected set; }
	object			Lock = new();
	public bool		ShowSender { get;set; } = false;
	public bool		ShowSubject { get;set; } = false;
	public int		BufferWidth => Console.BufferWidth;
	public string[]	Tags;

	public ConsoleLogView(bool showsender, bool showsubject)
	{
		ShowSender = showsender;
		ShowSubject = showsubject;

	}

	public void StartListening(Log log)
	{
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

	public void StopListening()
	{
		Log.Reported -= OnReported;
	}

	public void OnReported(LogMessage m)
	{
		lock(Lock)
		{
			if(Tags != null && m.Subject != null && m.Subject.Split(' ').Any(i => !Tags.Contains(i)))
				return;

			var prev = Console.ForegroundColor;

			Console.ForegroundColor = m.Severity switch
												 { 
													Log.Severity.SubLog => ConsoleColor.Green,
													Log.Severity.Error => ConsoleColor.Red,
													Log.Severity.Warning => ConsoleColor.Yellow,
													_ => prev
												 };

			Console.Write(new string(' ', m.Log.Depth * 4)); 

 			if(ShowSender && m.Sender != null)
				Console.Write(m.Sender.GetType().Name + " : ");

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
						Console.Write(new string(' ', m.Log.Depth * 4 + 4) + t.Substring(i, w));
						i += w;
					}
				
					Console.WriteLine();
				}
			}
				
			Console.ForegroundColor = prev;
		}
	}
}
