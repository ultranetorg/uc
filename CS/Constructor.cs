using System.Linq.Expressions;
using System.Reflection;

namespace Uccs;

public class Constructor
{
	public Dictionary<Type, uint>								Codes = [];
	public Dictionary<Type, Dictionary<uint, Func<object>>>		Ctors = [];

	public Constructor()
	{
	}

	public Constructor(Dictionary<Type, uint> codes, Dictionary<Type, Dictionary<uint, Func<object>>> contructors)
	{
		foreach(var i in codes)
			Codes.Add(i.Key, i.Value);

		foreach(var i in contructors)
		{
			Ctors[i.Key] = [];

			foreach(var j in i.Value)
				Ctors[i.Key].Add(j.Key, j.Value);
		}
	}

	public void Merge(Constructor constructor)
	{
		foreach(var i in constructor.Codes)
			if(!Codes.ContainsKey(i.Key))
				Codes.Add(i.Key, i.Value);

		foreach(var i in constructor.Ctors)
		{
			if(!Ctors.ContainsKey(i.Key))
				Ctors[i.Key] = [];

			foreach(var j in i.Value)
				if(!Ctors[i.Key].ContainsKey(j.Key))
					Ctors[i.Key].Add(j.Key, j.Value);
		}
	}

	public void Register<T>(Assembly assembly, Type enumclass, Func<string, string> getname, Action<T> setup = null, bool overwrite = false) where T : class
	{
		foreach(var i in assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(T))))
		{	
			if(Enum.TryParse(enumclass, getname(i.Name), out var c))
			{
				Codes[i] = (uint)c;

				if(!Ctors.ContainsKey(typeof(T)))
					Ctors[typeof(T)] = [];

				if(!overwrite && Ctors[typeof(T)].ContainsKey((uint)c))
					throw new ArgumentException();

				var e = Expression.New(i.GetConstructor([]));
				var l = Expression.Lambda<Func<T>>(e);
				var f = l.Compile();

				Ctors[typeof(T)][(uint)c] = () =>	{
															var r = f();
															setup?.Invoke(r);
															return r;
														};
			}
		}
	}

	public void Register<T>(Func<T> create) where T : class
	{
		if(!Ctors.ContainsKey(typeof(T)))
			Ctors[typeof(T)] = [];

		Ctors[typeof(T)][0] = create;
	}

	public void Register<B, T>(Func<T> create) where T : class
	{
		if(!Ctors.ContainsKey(typeof(B)))
			Ctors[typeof(B)] = [];

		Ctors[typeof(B)][0] = create;
	}

	public virtual object Construct(Type type, uint code)
	{
		var x = Ctors.GetValueOrDefault(type, null);
		
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