using System.Linq.Expressions;
using System.Reflection;

namespace Uccs;

public class Constructor
{
	public Dictionary<Type, uint>								Codes = [];
	public Dictionary<Type, Dictionary<uint, Func<object>>>		Contructors = [];

	public Constructor()
	{
	}

	public Constructor(Dictionary<Type, uint> codes, Dictionary<Type, Dictionary<uint, Func<object>>> contructors)
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

	public void Register<T>(Assembly assembly, Type enumclass, Func<string, string> getname, Action<T> setup = null, bool overwrite = false) where T : class
	{
		foreach(var i in assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(T))))
		{	
			if(Enum.TryParse(enumclass, getname(i.Name), out var c))
			{
				Codes[i] = (uint)c;

				if(!Contructors.ContainsKey(typeof(T)))
					Contructors[typeof(T)] = [];

				if(!overwrite && Contructors[typeof(T)].ContainsKey((uint)c))
					throw new ArgumentException();

				var e = Expression.New(i.GetConstructor([]));
				var l = Expression.Lambda<Func<T>>(e);
				var f = l.Compile();

				Contructors[typeof(T)][(uint)c] = () =>	{
															var r = f();
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

	public virtual object Construct(Type type, uint code)
	{
		var x = Contructors.GetValueOrDefault(type, null);
		
		if(x != null)
			return (x.GetValueOrDefault(code, null) ?? x.GetValueOrDefault((uint)0, null))?.Invoke();
		else 
			return null;
	}

	public virtual uint TypeToCode(Type type)
	{
		return Codes[type];
	}
}