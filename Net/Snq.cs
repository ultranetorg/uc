namespace Uccs.Net;

public class Snq : IBinarySerializable, IEquatable<Snq>  /// Scheme Net Path
{
					
	public string		Scheme { get; set; }
	public string		Net { get; set; }
	public string		Query { get; set; }

	const string		Postfix = $".{Iccn.Root}";
	//public bool Valid => !string.IsNullOrWhiteSpace(Scheme) && !string.IsNullOrWhiteSpace(Entity);

	public Snq()
	{
	}

	public Snq(string scheme, string net, string path)
	{
		Scheme = scheme;
		Net = net;
		Query = path;
	}

	public override string ToString()
	{
		return ToString(Scheme, Net, Query);
	}

	public static string ToCanonicalNet(string address)
	{

		if(address == Iccn.Root || address == null || address == string.Empty)
			return Iccn.Root;

 		if(!address.EndsWith(Postfix))
			return $"{address}{Postfix}";

		return address;
	}

	public static string ToString(string scheme, string net, string entity)
	{
		return $"{(scheme == null ? null : (scheme + ':'))}{net}{(entity == null ? null : ('/' + entity))}";
	}

	public static Snq Parse(string v)
	{
		Parse(v, out var s, out var z, out var e);

		return new Snq(s, z, e);
	}

	public static void Parse(string v, out string scheme, out string net, out string query)
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
			query = v.Substring(s);
		else
			query = null;
	}

	public override bool Equals(object o)
	{
		return o is Snq a && Equals(a);
	}

	public bool Equals(Snq o)
	{
		return o is not null && Scheme == o.Scheme && NetsEqual(Net, o.Net) && Query == o.Query;
	}

	public override int GetHashCode()
	{
		return Query.GetHashCode();
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as Snq);
	}

	public int CompareTo(Snq other)
	{
		var c = Scheme.CompareTo(other.Scheme);
		if(c != 0)
			return c;

		c = Net.CompareTo(other.Net);
		if(c != 0)
			return c;

		c = Query.CompareTo(other.Query);
		if(c != 0)
			return c;

		return 0;
	}

	public static bool operator ==(Snq a, Snq b)
	{
		return a is null && b is null || a is not null && a.Equals(b);
	}

	public static bool operator !=(Snq left, Snq right)
	{
		return !(left == right);
	}

	public void Write(Writer writer)
	{
		writer.WriteUtf8(Scheme);
		writer.WriteUtf8(Net);
		writer.WriteUtf8(Query);
	}

	public void Read(Reader reader)
	{
		Scheme	= reader.ReadUtf8();
		Net		= reader.ReadUtf8();
		Query	= reader.ReadUtf8();
	}

	public static bool NetsEqual(string x, string y)
	{
		if(x == null || x == string.Empty)
			x = Iccn.Root;

		if(y == null || y == string.Empty)
			y = Iccn.Root;

		var i = x.LastIndexOf(Postfix, StringComparison.InvariantCultureIgnoreCase);
		var j = y.LastIndexOf(Postfix, StringComparison.InvariantCultureIgnoreCase);

		if((i != -1 && i == x.Length - Postfix.Length) || (j != -1 && j == y.Length - Postfix.Length))
			return string.Compare(x, 0, y, 0, Math.Min(Math.Max(i, j), Math.Min(x.Length, y.Length)), true) == 0;
		else
			return string.Compare(x, 0, y, 0, Math.Min(x.Length, y.Length), true) == 0;
	}
}
