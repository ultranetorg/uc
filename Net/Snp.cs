namespace Uccs.Net;

public class Snp : IBinarySerializable, IEquatable<Snp>  /// Universal Network/Entity Locator
{
	public const string Common = "iccp";
						
	public string		Scheme { get; set; }
	public string		Net { get; set; }
	public string		Path { get; set; }

	//public bool Valid => !string.IsNullOrWhiteSpace(Scheme) && !string.IsNullOrWhiteSpace(Entity);

	public Snp()
	{
	}

	public Snp(string scheme, string net, string path)
	{
		Scheme = scheme;
		Net = net;
		Path = path;
	}

	public override string ToString()
	{
		return ToString(Scheme, Net, Path);
	}

	public static string ToString(string scheme, string net, string entity)
	{
		return $"{(scheme == null ? null : (scheme + ':'))}{net}{(entity == null ? null : ('/' + entity))}";
	}

	public static Snp Parse(string v)
	{
		Parse(v, out var s, out var z, out var e);

		return new Snp(s, z, e);
	}

	public static void Parse(string v, out string scheme, out string net, out string path)
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
			path = v.Substring(s);
		else
			path = null;
	}

	public override bool Equals(object o)
	{
		return o is Snp a && Equals(a);
	}

	public bool Equals(Snp o)
	{
		return o is not null && Scheme == o.Scheme && Net == o.Net && Path == o.Path;
	}

	public override int GetHashCode()
	{
		return Path.GetHashCode();
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as Snp);
	}

	public int CompareTo(Snp other)
	{
		var c = Scheme.CompareTo(other.Scheme);
		if(c != 0)
			return c;

		c = Net.CompareTo(other.Net);
		if(c != 0)
			return c;

		c = Path.CompareTo(other.Path);
		if(c != 0)
			return c;

		return 0;
	}

	public static bool operator ==(Snp a, Snp b)
	{
		return a is null && b is null || a is not null && a.Equals(b);
	}

	public static bool operator !=(Snp left, Snp right)
	{
		return !(left == right);
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Scheme);
		writer.WriteUtf8(Net);
		writer.WriteUtf8(Path);
	}

	public void Read(BinaryReader reader)
	{
		Scheme	= reader.ReadUtf8();
		Net		= reader.ReadUtf8();
		Path	= reader.ReadUtf8();
	}
}
