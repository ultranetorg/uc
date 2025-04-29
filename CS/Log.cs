using System.Collections;
using System.Net;

namespace Uccs;

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
	public List<Type>			TypesForExpanding { get; } = [];

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

	public void Dump(object o, Type type = null)
	{
		void dump(string name, object value, int tab)
		{
			if(value is null)
			{
				Report(new string(' ', tab * 3) + name + " :");
			}
			else if(value is ICollection e)
			{
				if(value is int[])
				{
					Report(new string(' ', tab * 3) + $"{name} : [{string.Join(", ", value as int[])}]");
				}
				else if(value is byte[])
				{
					Report(new string(' ', tab * 3) + $"{name} : {(value as byte[]).ToHex()}");
				}
				else if(value is IEnumerable<string> ||
						value is IEnumerable<IPAddress>)
				{
					Report(new string(' ', tab * 3) + $"{name} : [{string.Join(", ", value as IEnumerable<object>)}]");
				}
				else if(TypesForExpanding.Contains(value.GetType()))
				{
					Report(new string(' ', tab * 3) + $"{name} :");

					foreach(var i in value as IEnumerable)
					{
						dump(null, i, tab+1);
					}
				}
				else
					Report(new string(' ', tab * 3) + $"{name} : {{{e.Count}}}");
			}
			else if(TypesForExpanding.Contains(value.GetType()))
			{
				Report(new string(' ', tab * 3) + $"{name}");

				foreach(var i in value.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
				{
					dump(i.Name, i.GetValue(value), tab + 1);
				}
			}
			else
				Report(new string(' ', tab * 3) + $"{(name == null ? null : (name + " : " ))}{value}");
		}

		foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic && (type == null || i.DeclaringType == type)))
		{
			dump(i.Name, i.GetValue(o), 0);
		}
	}

	public void Dump<R>(IEnumerable<R> items, string[] columns, IEnumerable<Func<R, object>> gets, int tab = 0)
	{
		Dump(items, columns, gets.Select(g => new Func<R, int, object>((o, i) => g(o))), tab);
	}

	public void Dump<T>(IEnumerable<T> items, string[] columns, IEnumerable<Func<T, int, object>> gets, int tab = 0)
	{
		if(!items.Any())
		{	
			Report("No results");
			return;
		}

		object[,] t = new object[items.Count(), columns.Length];
		int[] w = columns.Select(i => i.TrimEnd('>').Length).ToArray();

		var ii = 0;

		foreach(var i in items)
		{
			var gi = 0;

			foreach(var g in gets)
			{
				t[ii, gi] = g(i, ii);
				
				if(t[ii, gi] != null)
				{	
					w[gi] = Math.Max(w[gi], t[ii, gi].ToString().Length);
				}

				gi++;
			}

			ii++;
		}

		var f = string.Join("  ", columns.Select((c, i) => $"{{{i},{(columns[i].EndsWith('>') ? "" : "-")}{w[i]}}}"));

		Report(new string(' ', tab * 3) + string.Format(f, columns.Select(i => i.TrimEnd('>')).ToArray()));
		Report(new string(' ', tab * 3) + string.Format(f, w.Select(i => new string('─', i)).ToArray()));
					
		//f = string.Join("  ", columns.Select((c, i) => $"{{{i},{w[i]}}}"));

		for(int i=0; i < items.Count(); i++)
		{
			Report(new string(' ', tab * 3) + string.Format(f, Enumerable.Range(0, columns.Length).Select(j => t[i, j]).ToArray()));
		}
	}

	public void DumpFixed<T>(IEnumerable<T> items, string[] columns, Func<T, object>[] gets, int tab = 0)
	{
		if(!items.Any())
		{	
			Report("No results");
			return;
		}

		var f = string.Join("  ", columns.Select((c, i) => $"{{{i},{(columns[i].EndsWith('>') ? "" : "-")}15}}"));

		Report(new string(' ', tab * 3) + string.Format(f, columns.Select(i => i.TrimEnd('>')).ToArray()));
		Report(new string(' ', tab * 3) + string.Format(f, columns.Select(i => new string('─', 15)).ToArray()));
					
		//f = string.Join("  ", columns.Select((c, i) => $"{{{i},{w[i]}}}"));

		foreach(var i in items)
		{
			Report(new string(' ', tab * 3) + string.Format(f, Enumerable.Range(0, columns.Length).Select(j => gets[j](i)).ToArray()));
		}
	}

	protected void Dump(Xon xon)
	{
		xon.Dump((n, l) => Report(new string(' ', (l+1) * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<string>(n, n.Value)))));
	}
}
