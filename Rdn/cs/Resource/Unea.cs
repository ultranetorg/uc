namespace Uccs.Rdn;

public class Unea : IBinarySerializable, IEquatable<Unea>  /// Univeral Network Entity Address
{
	public string			Scheme { get; set; }
	public string			Net { get; set; }
	public string			Entity{ get; set; }

	public bool				Valid => !string.IsNullOrWhiteSpace(Scheme) && !string.IsNullOrWhiteSpace(Entity);

	public Unea()
	{
	}

	public Unea(string scheme, string net, string entity)
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
		return (scheme == null ? null : (scheme + ':')) + net + (entity == null ? null : ('/' + entity));
	}

	public static Unea Parse(string v)
	{
		Parse(v, out var s, out var z, out var e);

		return new Unea(s, z, e);
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
		return o is Unea a && Equals(a);
	}

	public bool Equals(Unea o)
	{
		return Scheme == o.Scheme && Net == o.Net && Entity == o.Entity;
	}

 		public override int GetHashCode()
 		{
 			return Entity.GetHashCode();
 		}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as Unea);
	}

	public int CompareTo(Unea other)
	{
		if(Scheme.CompareTo(other.Scheme) != 0)
			return Scheme.CompareTo(other.Scheme);

		if(Net != other.Net)
			return Net.CompareTo(other.Net);

		if(Entity.CompareTo(other.Entity) != 0)
			return Entity.CompareTo(other.Entity);

		return 0;
	}

	public static bool operator == (Unea a, Unea b)
	{
		return a is null && b is null || a is not null && a.Equals(b);
	}

	public static bool operator != (Unea left, Unea right)
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
