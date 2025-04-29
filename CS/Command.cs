using System.Collections;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace Uccs;

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

}
