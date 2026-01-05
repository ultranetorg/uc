using System.Collections;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace Uccs;

public abstract class Command
{
	public class CommandAction
	{
		Command					Command;
		public string			Name;
		public string			LongName => Method.Name.Replace("_", null).ToLower();
		public string			Title => Method.Name.Replace("_", " ");
		public string[]			Names => [Name, LongName];
		public string			Description {get; set; }
		public Argument[]		Arguments {get; set; }
		public Func<object>		Execute;

		public string			NamesSyntax => string.Join('|', Names);
		public Argument			this[int argument] => Arguments[argument];

		MethodBase				Method;
		public Func<Example[]>	Examples;
		
		public string Syntax
		{
			get
			{
				var s = Command.Keyword;
	
				var used = new Dictionary<ArgumentType, int>();
	
				if(Arguments != null)
				{
					foreach(var i in Arguments)
						if(i.Name == null)
							s += $" {i.Type.Name}";
	
					foreach(var i in Arguments)
						if(i.Name != null)
							if(i.Type != null)
								s += $" {i.Name}={i.Type.Name}";
							else
								s += $" {i.Name}"; /// boolean
				}
				return s;
			}
		}
		
	
		public CommandAction(Command command, MethodBase method)
		{
			Command = command;
			Method = method;

			Examples = () =>	{
									var c = Command.Keyword;
	
									var used = new Dictionary<ArgumentType, int>();
	
									string nextexample(ArgumentType t)
									{
										if(!used.ContainsKey(t))
											used[t] = 0;
										else
											used[t]++;
	
										return t.Examples[used[t]];
									}
	
									if(Arguments != null)
									{
										foreach(var i in Arguments)
											if(i.Name == null)
												c += $" {nextexample(i.Type)}";
	
										foreach(var i in Arguments)
											if(i.Name != null)
												if(i.Type != null)
													c += $" {i.Name}={nextexample(i.Type)}";
												else
													c += $" {i.Name}";
									}
									
									return [new Example(null, c)];
								};
		}
	}

	public class ArgumentType
	{
		public string	Name;
		public string	Description;
		public string[] Examples;

		public string	Example => Examples[0];
		public string	Example1 => Examples[1];
		public string	Example2 => Examples[2];

		public ArgumentType(string name, string description, string[] example)
		{
			Name = name;
			Description = description;
			Examples = example;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public enum Flag
	{
		None,
		First = 1,
		Second = 2,
		Optional = 4,
		Multi = 8
	}

	public class Argument
	{
		public string		Name {get; set; }
		public ArgumentType	Type {get; set; }
		public string		Description {get; set; }
		public Flag			Flags {get; set; }

		public Argument[]	Arguments  {get; set; }

		public Argument(string name, ArgumentType type, string description, Flag flags = Flag.None)
		{
			Type = type;
			Name = name;
			Description = description;
			Flags = flags;
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

	public const string		FirstArg = "<first>";

	public string			Keyword => GetType().Name.Replace(nameof(Command), null).ToLower();
	public CommandAction[]	Actions => GetType().GetMethods().Where(i => i.ReturnParameter.ParameterType == typeof(CommandAction)).Select(i => i.Invoke(this, null)).Cast<CommandAction>().ToArray();

	public List<Xon>		Args;
	public static bool		ConsoleAvailable { get; protected set; }
	public Flow				Flow;

	public void				Report(string message) => Flow.Log?.Report(this, "   " + message);

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

	public string GetString(string paramenter, bool mandatory = true)
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

	public string GetString(string paramenter, string def)
	{
		var p = One(paramenter);

		if(p != null)
			return p.Get<string>();
		else
			return def;
	}

	public long GetLong(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	public int GetInt(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return int.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	public int GetInt(string paramenter, int def)
	{
		var p = One(paramenter);

		if(p != null)
			return int.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			return def;
	}

	public byte[] GetBytes(string paramenter, bool mandatory = true)
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

	public Version GetVersion(string paramenter, bool mandatory = true)
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
