using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public enum AddressFormat : ushort
{
    Bech32b = (byte)'b'<<8 | (byte)'b',
	Bech32t = (byte)'b'<<8 | (byte)'t',
}

public enum Algorithm : ushort
{
   Secp256k1 = (byte)'e'<< 8 | (byte)'a',
   Schnorr   = (byte)'e'<< 8 | (byte)'s',
}

public class AccountAddress : IComparable, IComparable<AccountAddress>, IEquatable<AccountAddress>, IBinarySerializable
{
	public const int			Length = 32;
	public virtual byte[]		Bytes { get; protected set; }
	public string				Tag { get; protected set; }
	string						Text;
	AddressFormat 				Format;// = AddressEncoder.FormatBech32m_Raw; 

	public AccountAddress()
	{
	}

 	public AccountAddress(byte[] bytes)
 	{
		if(bytes.Length != Length)
			throw new IntegrityException("Wrong length");
		
		Format = AddressFormat.Bech32b;
		Bytes = bytes;
 	}

 	public AccountAddress(byte[] bytes, string tag)
 	{
		if(bytes.Length != Length)
			throw new ArgumentException("Wrong length");
		
        if(!string.IsNullOrEmpty(tag) && tag.Length != Bech32.TagLength) 
            throw new ArgumentException("Invalid tag length.");

		Bytes = bytes;
		Tag = tag;

		if(Tag == null)
			Format = AddressFormat.Bech32b;
		else
			Format = AddressFormat.Bech32t;
 	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Format);
		if(Format == AddressFormat.Bech32t)
			writer.WriteASCII(Tag);
		writer.Write(Bytes);
	}

	public void Read(BinaryReader reader)
	{
		Format = reader.Read<AddressFormat>();
		if(Format == AddressFormat.Bech32t)
			Tag = reader.ReadASCII();
		Bytes = reader.ReadBytes(Length);
	}

	public static AccountAddress Parse(string text)
	{
		var a = Decode(text);

		return new AccountAddress(a.data, a.tag);
	}

	public override string ToString()
	{
		if(!Enum.IsDefined(Format))
			throw new InvalidOperationException();

		if(Text == null)
			Text = Encode(Format, Algorithm.Schnorr, Bytes, Tag);
		
		return  Text;
	}

	public static bool operator == (AccountAddress a, AccountAddress b)
	{
		return a is null && b is null || a.Equals(b);
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
		return a is not null && Uccs.Bytes.EqualityComparer.Equals(Bytes, a.Bytes);
	}

	public override int GetHashCode()
	{
		return Bytes[0] << 8 & Bytes[1];
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as AccountAddress);
	}

	public int CompareTo(AccountAddress address)
	{
		return Uccs.Bytes.Comparer.Compare(Bytes, address.Bytes);
	}

    public static string Encode(AddressFormat format, Algorithm algorithm, byte[] data, string tag = null)
    {
        string body = Bech32.Encode(data, tag);

        return $"{body}{(char)((ushort)format >> 8)}{(char)(byte)format}{(char)((ushort)algorithm >> 8)}{(char)(byte)algorithm}";
    }

    public static (AddressFormat format, Algorithm algorithm, string tag, byte[] data) Decode(string address)
    {
        if(string.IsNullOrEmpty(address) || address.Length < 12)
            throw new FormatException("Invalid address length.");

        var f	= (AddressFormat)(char.ToLowerInvariant(address[address.Length-4]) << 8 | char.ToLowerInvariant(address[address.Length-3]));
        var a	= (Algorithm)	 (char.ToLowerInvariant(address[address.Length-2]) << 8 | char.ToLowerInvariant(address[address.Length-1]));

        if(f != AddressFormat.Bech32t && f != AddressFormat.Bech32b)
            throw new FormatException("Unsupported address format.");

        Bech32.TryDecode(address.AsSpan(0, address.Length-4), f == AddressFormat.Bech32t, out var data, out string tag);

        return (f, a, tag, data);
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
