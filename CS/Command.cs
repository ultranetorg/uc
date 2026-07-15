using System.Collections;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace Uccs;

public class CommandAction
{
	Command					Command;
	//public string			Name;
	public string[]			Names => Method.Name.ToLower().Split('_');
	public string			Name => (Names.Length > 1 ? Names[1] : Names[0]);
	public string			LongName => Names[0];
	public string			Title => Method.Name.Split('_')[0];
	public string			Description {get; set; }
	public Argument[]		Arguments {get; set; }
	public Func<object>		Execute;
	public bool				IsDefault;

	public string			NamesSyntax => string.Join('|', Names);
	public Argument			this[int argument] => Arguments[argument];

	MethodBase				Method;
	public Func<Example[]>	Examples;

	public string Syntax
	{
		get
		{
			var s = $"{Command.Keyword} {Name}";
	
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
								var c = $"{Command.Keyword} {Name}";
	
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
										if(i.Name == null && i.Type != null)
											c += $" {nextexample(i.Type)}";

								foreach(var i in Arguments)
								{
									var a = i.Arguments == null ? i : i.Arguments[0];

									if(a.Name != null)
										if(a.Type != null)
											c += $" {a.Name}={nextexample(a.Type)}";
										else
											c += $" {a.Name}";
								}
							}

			return [new Example(null, c)];
							};
	}

	public override string ToString()
	{
		return Method.Name;
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

	public ArgumentType(string name, string description, object[] example)
	{
		Name = name;
		Description = description;
		Examples = example.Select(i => i.ToString()).ToArray();
	}

	public override string ToString()
	{
		return Name;
	}
}

[Flags]
public enum ArgumentFlag
{
	Optional = 1,
	Multi = 2,
}

public class Argument
{
	public string			Name {get; set; }
	public ArgumentType		Type {get; set; }
	public string			Description {get; set; }
	public ArgumentFlag		Flags {get; set; }
	public object			Default {get; set; }	

	public Argument[]		Arguments  {get; set; }

	public Argument(string name, ArgumentType type, string description, ArgumentFlag flags = 0, object @default = null, Argument[] arguments = null)
	{
		Type = type;
		Name = name;
		Description = description;
		Flags = flags;
		Default = @default;
		Arguments = arguments;
	}

	public override string ToString()
	{
		return Name;
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

public abstract class Command
{
	public const string		ConfirmationArg = "_confirmation";
	public virtual string[]	ControlArguments => [ConfirmationArg];

	public string			Keyword => GetType().Name.Replace(nameof(Command), null).ToLower();
	public CommandAction[]	Actions => GetType().GetMethods().Where(i => i.ReturnParameter.ParameterType == typeof(CommandAction) && i.Name != nameof(GetAction) && i.Name != nameof(GetDefaultAction)).Select(i => i.Invoke(this, null)).Cast<CommandAction>().ToArray();

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

	public CommandAction GetAction(string name)
	{ 
		var c = GetType().GetMethods().FirstOrDefault(i => i.ReturnParameter.ParameterType == typeof(CommandAction) && i.Name != nameof(GetAction) && i.Name != nameof(GetDefaultAction) && ContainsWord(i.Name, name));

		return c?.Invoke(this, null) as CommandAction;
	}

	public CommandAction GetDefaultAction()
	{ 
		var c = GetType().GetMethods().Where(i => i.ReturnParameter.ParameterType == typeof(CommandAction) && i.Name != nameof(GetAction) && i.Name != nameof(GetDefaultAction));

		return c.Count() == 1 ? c.First().Invoke(this, null) as CommandAction : null;
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

	public static bool ContainsWord(string source, string word)
	{
		if(string.IsNullOrEmpty(source) || string.IsNullOrEmpty(word))
			return false;

		int wordLength = word.Length;
		int sourceLength = source.Length;
		int i = 0;

		while((i = source.IndexOf(word, i, StringComparison.InvariantCultureIgnoreCase)) != -1)
		{
			bool isLeftValid = (i == 0) || (source[i - 1] == '_');

			bool isRightValid = (i + wordLength == sourceLength) || (source[i + wordLength] == '_');

			if(isLeftValid && isRightValid)
			{
				return true;
			}

			i += 1;
		}

		return false;
	}

	public bool Has(string paramenter)
	{
		return One(paramenter) != null;
	}

	public bool GetBool(string paramenter, bool def)
	{
		var p = One(paramenter);

		if(p != null)
		{	
			if(string.Compare(p.Get<string>(), "yes", true) == 0)
				return true;
			else if(string.Compare(p.Get<string>(), "no", true) == 0)
				return false;
			else
				throw new SyntaxException($"Parameter '{paramenter}' has incorrect value");
		}
		else
			return def;
	}

	public string GetString(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return p.Get<string>();
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	public string GetString(string paramenter, string def)
	{
		var p = One(paramenter);

		if(p != null)
			return p.Get<string>();
		else
			return def;
	}

	public byte GetByte(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return byte.Parse(p.Get<string>());
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	public long GetLong(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
	}

	public long GetLong(string paramenter, long def)
	{
		var p = One(paramenter);

		if(p != null)
			return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
		else
			return def;
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

	public byte[] GetBytes(string paramenter, byte[] def)
	{
		var p = One(paramenter);

		if(p != null)
			return p.Get<string>().FromHex();
		else
			return def;
	}

	public byte[] GetBytes(string paramenter)
	{
		var p = One(paramenter);

		if(p != null)
			return p.Get<string>().FromHex();
		else
			throw new SyntaxException($"Parameter '{paramenter}' not provided");
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
