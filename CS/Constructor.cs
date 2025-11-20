using System.Reflection;

namespace Uccs;

public class Constructor
{
	public Dictionary<Type, byte>								Codes = [];
	public Dictionary<Type, Dictionary<byte, Func<object>>>		Contructors = [];

	public Constructor()
	{
	}

	public Constructor(Dictionary<Type, byte> codes, Dictionary<Type, Dictionary<byte, Func<object>>> contructors)
	{
		foreach(var i in codes)
			Codes.Add(i.Key, i.Value);

		foreach(var i in contructors)
		{
			Contructors[i.Key] = [];

			foreach(var j in i.Value)
				Contructors[i.Key].Add(j.Key, j.Value);
		}
	}

	public void Register<T>(Type enumclass, Func<string, string> getname, Action<T> setup = null) where T : class
	{
		var assembly = Assembly.GetCallingAssembly();

		foreach(var i in assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(T))))
		{	
			if(Enum.TryParse(enumclass, getname(i.Name), out var c))
			{
				Codes[i] = (byte)c;
				var x = i.GetConstructor([]);

				if(!Contructors.ContainsKey(typeof(T)))
					Contructors[typeof(T)] = [];

				if(Contructors[typeof(T)].ContainsKey((byte)c))
					throw new ArgumentException();

				Contructors[typeof(T)][(byte)c] = () =>	{
															var r = x.Invoke(null) as T;
															setup?.Invoke(r);
															return r;
														};
			}
		}
	}

	public void Register<T>(Func<T> create) where T : class
	{
		if(!Contructors.ContainsKey(typeof(T)))
			Contructors[typeof(T)] = [];

		Contructors[typeof(T)][0] = create;
	}

	public void Register<B, T>(Func<T> create) where T : class
	{
		if(!Contructors.ContainsKey(typeof(B)))
			Contructors[typeof(B)] = [];

		Contructors[typeof(B)][0] = create;
	}

	public virtual object Constract(Type type, byte code)
	{
		var x = Contructors.GetValueOrDefault(type, null);
		
		if(x != null)
			return (x.GetValueOrDefault(code, null) ?? x.GetValueOrDefault((byte)0, null))?.Invoke();
		else 
			return null;
	}

	public virtual byte TypeToCode(Type type)
	{
		return Codes[type];
	}
}