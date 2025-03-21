﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

/// <summary>
///		/domain/resource
/// </summary>

// 	public enum ResourceType
// 	{
// 		None, Variable, Constant
// 	}

public class Ura : IBinarySerializable, IEquatable<Ura>, IComparable, IComparable<Ura>, ITextSerialisable
{
//	public ResourceType			Type { get; set; }
	public string				Net { get; set; }
	public string				Domain { get; set; }
	public string				Resource { get; set; }

	public const string			Scheme = "ura";
	//public string				Uri => $"{Scheme}:{Net}{(Net != null ? ":" : null)}{Domain}/{Resource}";
	//public static ResourceType	SchemeToType(string s) => s[2] switch {'v' => ResourceType.Variable, 'c' => ResourceType.Constant};

	public bool					Valid => !string.IsNullOrWhiteSpace(Domain) && !string.IsNullOrWhiteSpace(Resource);

	public Ura()
	{
	}

	public Ura(string domain, string resource)
	{
		Domain = domain;
		Resource = resource;
	}

	public Ura(string net, string domain, string resource)
	{
		Net = net;
		Domain = domain;
		Resource = resource;
	}

	public Ura(Ura a)
	{
		Net			= a.Net;
		Domain		= a.Domain;
		Resource	= a.Resource;
	}

	public override string ToString()
	{
		return Unea.ToString(Scheme, Net, $"{Domain}/{Resource}");
	}

	public override bool Equals(object o)
	{
		return o is Ura a && Equals(a);
	}

	public bool Equals(Ura o)
	{
		return o is not null && Net == o.Net && Domain == o.Domain && Resource == o.Resource;
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
		var c = Net.CompareTo(o.Net);

		if(c != 0)
			return c;

		c = Domain.CompareTo(o.Domain);

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
		Parse(text, out string p, out string z, out string d, out string r);
		Net = z;
		Domain = d;
		Resource = r;
	}

	public static void Parse(string v, out string scheme, out string net, out string domain, out string resource)
	{
		int i;
		
		Unea.Parse(v, out scheme, out net, out string o);

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

	public static Ura Parse(string v)
	{
		Parse(v, out var s, out var z, out var d, out var r);

		return new Ura(z, d, r);
	}

	public static Ura ParseAR(string v)
	{
		var a = new Ura();
		
		var i = v.IndexOf('/');

		a.Domain = v.Substring(0, i);
		a.Resource = v.Substring(i+1);

		return a;
	}

	public void Write(BinaryWriter w)
	{
		w.Write((byte)(Net != null ? 0b1 : 0));
		
		if(Net != null)
			w.WriteUtf8(Net);
		
		w.WriteUtf8(Domain);
		w.WriteUtf8(Resource);
	}

	public void Read(BinaryReader r)
	{
		var b = r.ReadByte();
		
		if((b & 0b1) != 0) 
			Net = r.ReadUtf8();
		
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
