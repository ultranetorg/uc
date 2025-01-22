using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Uccs
{
	public abstract class Command
	{
		public string			Keyword => GetType().Name.Replace(nameof(Command), null).ToLower();
		public CommandAction[]	Actions => GetType().GetMethods().Where(i => i.ReturnParameter.ParameterType == typeof(Command.CommandAction)).Select(i => i.Invoke(this, null)).Cast<Command.CommandAction>().ToArray();

		public class Help
		{
			public class Argument
			{
				public string	Name {get; set; }
				public string	Description {get; set; }

				public Argument[]	Arguments  {get; set; }

				public Argument(string name, string description)
				{
					Name = name;
					Description = description;
				}
			}

			public class Example
			{
				public string Description  {get; set; }
				public string Code  {get; set; }

				public Example(string description, string code)
				{
					Description = description;
					Code = code;
				}
			}

			//public string		Title {get; set; }
			public string		Description {get; set; }
			public string		Syntax {get; set; }
			public Argument[]	Arguments {get; set; }
			public Example[]	Examples {get; set; } 
		}

		public class CommandAction
		{
			public string			Name;
			public string			LongName => Method.Name.Replace("_", null).ToLower();
			public string			Title => Method.Name.Replace("_", " ");
			public string[]			Names => [Name, LongName];
			public Help				Help;
			public Func<object>		Execute;

			public string			NamesSyntax => string.Join('|', Names);

			MethodBase				Method;

			public CommandAction(MethodBase method)
			{
				Method = method;
			}
		}

		public List<Xon>			Args;
		public static bool			ConsoleAvailable { get; protected set; }
		public Flow					Flow;

		protected virtual Type[]	TypesForExpanding { get; } = [];

		public void					Report(string message) => Flow.Log?.Report(this, "   " + message);

		static Command()
		{
			try
			{
				var p = Console.KeyAvailable;
				ConsoleAvailable = true;
			}
			catch(Exception)
			{
				ConsoleAvailable = false;
			}
		}

		protected Command()
		{
		}

		protected Command(List<Xon> args, Flow flow)
		{
			Args = args;
			Flow = flow;;
		}

		public Xon One(string path)
		{
			var names = path.Split('/');

			var i = names.GetEnumerator();
			
			i.MoveNext();

			var p = Args.FirstOrDefault(j => j.Name.Equals(i.Current));

			if(p != null)
			{
				while(i.MoveNext())
				{
					p = p.Nodes.FirstOrDefault(j => j.Name.Equals(i.Current));

					if(p == null)
					{
						return null;
					}
				}
			}
			return p;	 
		}
		
//		protected ResourceIdentifier GetResourceIdentifier(string a, string id)
//		{
//			if(Has(a))
//				return new ResourceIdentifier(GetResourceAddress(a));
//
//			if(Has(id))
//				return new ResourceIdentifier(ResourceId.Parse(GetString(id)));
//
//			throw new SyntaxException("address or id required");
//		}
//		
//		protected ResourceIdentifier ResourceIdentifier
//		{
//			get
//			{
//				if(Has("a"))
//					return new ResourceIdentifier(GetResourceAddress("a"));
//
//				if(Has("id"))
//					return new ResourceIdentifier(ResourceId.Parse(GetString("id")));
//
//				throw new SyntaxException("address or id required");
//			}
//		}

		public bool Has(string paramenter)
		{
			return One(paramenter) != null;
		}

		protected string GetString(string paramenter, bool mandatory = true)
		{
			var p = One(paramenter);

			if(p != null)
				return p.Get<string>();
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected string GetString(string paramenter, string def)
		{
			var p = One(paramenter);

			if(p != null)
				return p.Get<string>();
			else
				return def;
		}

		protected long GetLong(string paramenter)
		{
			var p = One(paramenter);

			if(p != null)
				return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected int GetInt(string paramenter)
		{
			var p = One(paramenter);

			if(p != null)
				return int.Parse(p.Get<string>(), NumberStyles.AllowThousands);
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected int GetInt(string paramenter, int def)
		{
			var p = One(paramenter);

			if(p != null)
				return int.Parse(p.Get<string>(), NumberStyles.AllowThousands);
			else
				return def;
		}

		protected byte[] GetBytes(string paramenter, bool mandatory = true)
		{
			var p = One(paramenter);

			if(p != null)
				return p.Get<string>().FromHex();
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected Version GetVersion(string paramenter, bool mandatory = true)
		{
			var p = One(paramenter);

			if(p != null)
				return Version.Parse(p.Get<string>());
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
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
			items = [..items];
			gets = [..gets];

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

		protected void Dump(Xon xon)
		{
			xon.Dump((n, l) => Report(new string(' ', (l+1) * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<string>(n, n.Value)))));
		}
	}
}
