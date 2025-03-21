﻿namespace Uccs;

public delegate void ReportedDelegate(LogMessage s);

public class LogMessage
{
	public object		Sender;
	public string		Subject { get; set; }
	public Log.Severity Severity { get; set; }
	public string[]		Text { get; set; }
	public Log			Log;

	public override string ToString()
	{
		return	$"{(Severity != Uccs.Log.Severity.Info ? ("!!! " + Severity + " : ") : null)}" +
				$"{(Sender != null ? Sender.GetType().Name + " : " : null)}" +
				$"{(Subject != null ? Subject : null)}" +
				(Subject != null && Text != null ? " : " : null) +
				$"{(Text != null ?  string.Join("; ", Text) : null)}";
	}
}

public class Log
{
	public enum Severity
	{
		None, Info, Warning, Error, SubLog
	}

	public List<LogMessage>		Messages = new List<LogMessage>(1500);
	public ReportedDelegate		Reported;
	public Log					Parent;
	string						Name;

	public int Depth
	{
		get
		{
			var r = this;
			int d = 0;
		
			while(r.Parent != null)
			{
				r = r.Parent;
				d++;
			}

			return d;
		}
	}

	public Log SubLog(string name)
	{
		Report(null, name, Severity.SubLog, null);

		var l = new Log{Name = name, Parent = this};

		return l;
	}

	protected void Report(object sender, string subject, Severity severity,  IEnumerable<string> a)
	{
		var m = new LogMessage{Log = this, Severity = severity, Sender = sender, Subject = subject, Text = a?.ToArray()};

		var r = this;
		
		while(r.Parent != null)
			r = r.Parent;
		
		lock(r.Messages)
		{
			r.Messages.Add(m);

			if(r.Messages.Count > 1500)
				r.Messages.RemoveRange(0, 1000);
		
			r.Reported?.Invoke(m);
		}
	}

	public void Report(string a)
	{
		Report(null, null, Severity.Info, new[] {a});
	}

	public void Report(object sender, string a)
	{
		Report(sender, null, Severity.Info, new[] {a});
	}

	public void Report(object sender, string subject, string a)
	{
		Report(sender, subject, Severity.Info, new[] {a});
	}

	public void Report(object sender, string subject, IEnumerable<string> a)
	{
		Report(sender, subject, Severity.Info, a);
	}

	public void ReportError(object sender, string a)
	{
		Report(sender, null, Severity.Error, new[]{a});
	}

	public void ReportError(string a)
	{
		Report(null, null, Severity.Error, new[]{ a });
	}

	public void ReportWarning(object sender, string a)
	{
		Report(sender, null, Severity.Warning, new[]{a});
	}

	public void ReportWarning(string a)
	{
		Report(null, null, Severity.Warning, new[]{a});
	}

	public void ReportWarning(object sender, string subject, string message, Exception ex)
	{
		var t = new List<string>(){message};

		var e = ex;

		while(e != null)
		{
			t.Add(e.Message);
			e = e.InnerException;
		}

		if(ex is AggregateException a)
		{
			foreach(var i in a.InnerExceptions)
			{
				t.Add(i.Message);
			}
		}
		
		Report(sender, subject, Severity.Warning, t.ToArray());
	}

	public void ReportError(object sender, string message, Exception ex)
	{
		var t = new List<string>(){message};

		var e = ex;

		while(e != null)
		{
			t.Add(e.Message);
			e = e.InnerException;
		}

		if(ex is AggregateException a)
		{
			foreach(var i in a.InnerExceptions)
			{
				t.Add(i.Message);
			}
		}

		
		Report(sender, null, Severity.Error, t.ToArray());
	}
}
