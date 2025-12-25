using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public class AccountAddress : IComparable, IComparable<AccountAddress>, IEquatable<AccountAddress>, IBinarySerializable
{
	public const string		Prefix = "0x";
	public const int		Length = 20;
	public virtual byte[]	Bytes { get; protected set; }
	public static readonly	AccountAddress Zero = new AccountAddress(new byte[Length]);
	//public byte[]			Prefix => Bytes.Take(Consensus.PrefixLength).ToArray();

	//public static implicit operator byte[] (AccountAddress d) => d.Bytes;
	
	public byte	this[int k] => Bytes[k];
 

	public AccountAddress()
	{
	}

 	public AccountAddress(byte[] b)
 	{
		if(b.Length == Length)
			Bytes = b;
		else
			throw new IntegrityException("Wrong length");
 	}

	public void Write(BinaryWriter w)
	{
		w.Write(Bytes);
	}

	public void Read(BinaryReader r)
	{
		Bytes = r.ReadBytes(Length);
	}

	public static AccountAddress Parse(string pubaddr)
	{
		if(pubaddr[0] == '0' && pubaddr[1] == 'x')
			return new AccountAddress(pubaddr.Substring(2).FromHex());
		else
			throw new FormatException();
	}

	public override string ToString()
	{
		return Bytes != null ? Prefix + Bytes.ToHex() : "";
	}

	public static bool operator == (AccountAddress a, AccountAddress b)
	{
		return a is null && b is null || a is not null && a.Equals(b);
	}

	public static bool operator != (AccountAddress a, AccountAddress b)
	{
		return !(a == b);
	}

	public override bool Equals(object o)
	{
		return o is AccountAddress a && Equals(a);  
	}

	public bool Equals(AccountAddress a)
	{
		return a is not null && a.Bytes.SequenceEqual(Bytes);
	}

	public override int GetHashCode()
	{
		return Bytes[0] << 8 & Bytes[1];
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as AccountAddress);
	}

	public int CompareTo(AccountAddress obj)
	{
		var x = Bytes;
		var y = obj.Bytes;

		var len = Math.Min(x.Length, y.Length);

		for (var i = 0; i < len; i++)
		{
		    var c = x[i].CompareTo(y[i]);
		    if (c != 0)
		    {
		        return c;
		    }
		}

		return x.Length.CompareTo(y.Length);
	}
}

public class AccountJsonConverter : JsonConverter<AccountAddress>
{
	public override AccountAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return AccountAddress.Parse(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, AccountAddress value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
