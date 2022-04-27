using System;
using System.Collections.Generic;
using System.IO;

namespace UC
{
	public delegate void ReportedDelegate(LogMessage s);

	public class LogMessage
	{
		public object		Sender;
		public string		Subject;
		public Log.Severity Severity;
		public string[]		Text;

		public override string ToString()
		{
			return	$"{(Severity != UC.Log.Severity.Info ? ("!!! " + Severity + " : ") : null)}" +
					$"{(Sender != null ? Sender.GetType().Name + " : " : null)}" +
					$"{(Subject != null ? Subject + " : " : null)}" +
					$"{(Text != null ? string.Join("; ", Text) : null)}";
		}
	}

	public class Log
	{
		public enum Severity
		{
			Null, Info, Warning, Error
		}

		public List<LogMessage>		Messages = new List<LogMessage>();
		public ReportedDelegate		Reported;
		public Stream				Stream{ set { Writer = new StreamWriter(value); }  }
		public TextWriter			Writer;

		protected void Report(object sender, string subject, Severity severity, string[] a)
		{
			var m = new LogMessage {Severity = severity, Sender = sender, Subject = subject, Text = a};
			
			lock(Messages)
			{
				Messages.Add(m);
	
				if(Messages.Count > 1000)
					Messages.RemoveRange(0, Messages.Count - 1000);
			}

			if(Writer != null)
			{
				Writer.WriteLine(m.ToString());
				Writer.Flush();
			}
					
			Reported?.Invoke(m);
		}

		public void Report(object sender, string subject)
		{
			Report(sender, subject, Severity.Info, null);
		}

		public void Report(object sender, string subject, string a)
		{
			Report(sender, subject, Severity.Info, new[] {a});
		}

		public void Report(object sender, string subject, string a, string b)
		{
			Report(sender, subject, Severity.Info, new[] {a, b});
		}

		public void Report(object sender, string subject, string a, string b, string c)
		{
			Report(sender, subject, Severity.Info, new[] {a, b, c});
		}

		public void Report(object sender, string subject, string[] a)
		{
			Report(sender, subject, Severity.Info, a);
		}

		public void ReportError(object sender, string a)
		{
			Report(sender, null, Severity.Error, new[]{ a });
		}

		public void ReportWarning(object sender, string a)
		{
			Report(sender, null, Severity.Warning, new[]{a});
		}

		public void ReportWarning(object sender, string subject, string message, Exception ex)
		{
			var t = new List<string>();

			if(ex is AggregateException a)
			{
				foreach(var i in a.InnerExceptions)
				{
					t.Add(i.Message);
				}
			}
			else
			{
				var e = ex;
				
				while(e != null)
				{
					t.Add(e.Message);
					e = e.InnerException;
				}
			}
			
			Report(sender, subject, Severity.Warning, t.ToArray());
		}

		public void ReportError(object sender, string message, Exception ex)
		{
			var t = new List<string>();

			if(ex is AggregateException a)
			{
				foreach(var i in a.InnerExceptions)
				{
					t.Add(i.Message);
				}
			}
			else
			{
				var e = ex;
				
				while(e != null)
				{
					t.Add(e.Message);
					e = e.InnerException;
				}
			}
			
			Report(sender, null, Severity.Error, t.ToArray());
		}
	}
}
