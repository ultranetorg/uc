using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public enum AddressFormat : ushort
{
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
	AddressFormat 				Format = AddressFormat.Bech32t; 

	public AccountAddress()
	{
	}

 	public AccountAddress(byte[] bytes)
 	{
		if(bytes.Length != Length)
			throw new IntegrityException("Wrong length");
		
		Bytes = bytes;
 	}

 	public AccountAddress(byte[] bytes, string tag)
 	{
		if(bytes.Length != Length)
			throw new ArgumentException("Wrong length");
		
		if(tag != null)
		{	
			if(string.IsNullOrEmpty(tag) || tag.Length == 0 || tag.Length > Bech32.MaxTagLength) 
				throw new ArgumentException("Invalid tag length.");

			if(tag.Any(i => !Bech32.Alphanumeric.Contains(i))) 
			    throw new ArgumentException("Invalid tag format");
		}

		Bytes = bytes;
		Tag = tag;
 	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Format);
		writer.Write(Algorithm.Schnorr);
		writer.WriteASCII(Tag);
		writer.Write(Bytes);
	}

	public void Read(BinaryReader reader)
	{
		Format = reader.Read<AddressFormat>();
		reader.Read<Algorithm>();
		Tag = reader.ReadASCII();
		Bytes = reader.ReadBytes(Length);
	}

	public static AccountAddress Parse(string address)
	{
        if(string.IsNullOrEmpty(address) || address.Length < 6) ///[data>1][taglength=1][format=2][algorithm=2]
            throw new FormatException("Invalid address length");

        var f	= (AddressFormat)(char.ToLowerInvariant(address[address.Length-4]) << 8 | char.ToLowerInvariant(address[address.Length-3]));
        var a	= (Algorithm)	 (char.ToLowerInvariant(address[address.Length-2]) << 8 | char.ToLowerInvariant(address[address.Length-1]));

        if(f != AddressFormat.Bech32t)
            throw new FormatException("Unsupported address format");

        if(a != Algorithm.Schnorr)
            throw new FormatException("Unsupported address format");

        if(!Bech32.TryDecode(address.AsSpan(0, address.Length - 4), out var data, out string tag))
			throw new FormatException("Invalid address format");

		return new AccountAddress(data, tag);
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
		return a is not null && Uccs.Bytes.EqualityComparer.Equals(Bytes, a.Bytes) && Format == a.Format;
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
