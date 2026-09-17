using System.Text.Json;
using System.Text.Json.Serialization;
using Uccs.Net;

namespace Uccs.Rdn;

/// <summary>
///		net/domain/resource
/// </summary>

public class Ura : IBinarySerializable, IEquatable<Ura>, IComparable, IComparable<Ura>, ITextSerialisable
{
	public string				Domain { get; set; }
	public string				Resource { get; set; }

	public bool					Valid => !string.IsNullOrWhiteSpace(Domain) && !string.IsNullOrWhiteSpace(Resource);

	public Ura()
	{
	}

	public Ura(string domain, string resource)
	{
		Domain = domain;
		Resource = resource;
	}

	public Ura(Snq address)
	{
		Parse(address.Query, out var d, out var r);
		
		Domain = d;
		Resource = r;
	}

	public Ura(Ura a)
	{
		Domain		= a.Domain;
		Resource	= a.Resource;
	}

	public static string ToString(string domain, string resource)
	{
		return Snq.ToString(Iccp.Scheme, null, domain + (resource != null ? $"/{resource}" : null));
	}

	public override string ToString()
	{
		return ToString(Domain, Resource);
	}

	public Snq ToSnq()
	{
		return new Snq(Iccp.Scheme, null, Domain + (Resource != null ? $"/{Resource}" : null));
	}

	public override bool Equals(object o)
	{
		return o is Ura a && Equals(a);
	}

	public bool Equals(Ura o)
	{
		return o is not null && Domain == o.Domain && Resource == o.Resource;
	}

 	public override int GetHashCode()
 	{
 		return Domain.GetHashCode();
 	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as Ura);
	}

	public int CompareTo(Ura o)
	{
		var c = Domain.CompareTo(o.Domain);

		if(c != 0)
			return c;

		return Resource.CompareTo(o.Resource);
	}

	public static bool operator == (Ura left, Ura right)
	{
		return left is null && right is null || left is not null && left.Equals(right);
	}

	public static bool operator != (Ura left, Ura right)
	{
		return !(left == right);
	}


	public void Read(string text)
	{
		Parse(text, out string d, out string r);
		Domain = d;
		Resource = r;
	}

	public static void Parse(string v, out string domain, out string resource)
	{
		int i;
		
		Snq.Parse(v, out _, out _, out string o);

		var e = o.IndexOf('/');
			
		if(e != -1)
		{
			domain = o.Substring(0, e);
			i = e + 1;
		}
		else
		{
			domain = o;
			i = -1;
		}

		if(i != -1)
			resource = o.Substring(i);
		else
			resource = null;
	}

	public static void ParseQuery(string query, out string domain, out string resource)
	{
		int i;

		var e = query.IndexOf('/');
			
		if(e != -1)
		{
			domain = query.Substring(0, e);
			i = e + 1;
		}
		else
		{
			domain = query;
			i = -1;
		}

		if(i != -1)
			resource = query.Substring(i);
		else
			resource = null;
	}

	public static Ura Parse(string v)
	{
		Parse(v, out var d, out var r);

		return new Ura(d, r);
	}

	public static Ura ParseQuery(string v)
	{
		var a = new Ura();
		
		var i = v.IndexOf('/');

		a.Domain = v.Substring(0, i);
		a.Resource = v.Substring(i+1);

		return a;
	}

	public void Write(Writer w)
	{
		w.WriteUtf8(Domain);
		w.WriteUtf8(Resource);
	}

	public void Read(Reader r)
	{
		Domain		= r.ReadUtf8();
		Resource	= r.ReadUtf8();
	}
}

public class UraJsonConverter : JsonConverter<Ura>
{
	public override Ura Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Ura.Parse(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, Ura value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
