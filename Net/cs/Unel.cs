namespace Uccs.Net;

public class Unel : IBinarySerializable, IEquatable<Unel>  /// Universal Network/Entity Locator
{
	public string Scheme { get; set; }
	public string Net { get; set; }
	public string Entity { get; set; }

	//public bool Valid => !string.IsNullOrWhiteSpace(Scheme) && !string.IsNullOrWhiteSpace(Entity);

	public Unel()
	{
	}

	public Unel(string scheme, string net, string entity)
	{
		Scheme = scheme;
		Net = net;
		Entity = entity;
	}

	public override string ToString()
	{
		return ToString(Scheme, Net, Entity);
	}

	public static string ToString(string scheme, string net, string entity)
	{
		return $"{(scheme == null ? null : (scheme + ':'))}{net}{(entity == null ? null : ('/' + entity))}";
	}

	public static Unel Parse(string v)
	{
		Parse(v, out var s, out var z, out var e);

		return new Unel(s, z, e);
	}

	public static void Parse(string v, out string scheme, out string net, out string entity)
	{
		int s = 0;
		var i = v.IndexOfAny([':', '/']);

		if(i != -1 && v[i] == ':')
		{
			scheme = v.Substring(0, i);
			s = i + 1;
		}
		else
			scheme = null;

		i = v.IndexOf('/', s);

		if(i != -1)
		{
			if(i != s)
			{
				net = v.Substring(s, i - s);
			}
			else
				net = null;

			s = i + 1;
		}
		else
			net = v.Substring(s);

		if(i != -1)
			entity = v.Substring(s);
		else
			entity = null;
	}

	public override bool Equals(object o)
	{
		return o is Unel a && Equals(a);
	}

	public bool Equals(Unel o)
	{
		return Scheme == o.Scheme && Net == o.Net && Entity == o.Entity;
	}

	public override int GetHashCode()
	{
		return Entity.GetHashCode();
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as Unel);
	}

	public int CompareTo(Unel other)
	{
		var c = Scheme.CompareTo(other.Scheme);
		if(c != 0)
			return c;

		c = Net.CompareTo(other.Net);
		if(c != 0)
			return c;

		c = Entity.CompareTo(other.Entity);
		if(c != 0)
			return c;

		return 0;
	}

	public static bool operator ==(Unel a, Unel b)
	{
		return a is null && b is null || a is not null && a.Equals(b);
	}

	public static bool operator !=(Unel left, Unel right)
	{
		return !(left == right);
	}

	public void Write(BinaryWriter w)
	{
		throw new NotImplementedException();
	}

	public void Read(BinaryReader r)
	{
		throw new NotImplementedException();
	}
}
