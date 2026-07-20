using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public enum KeyFormat : ushort
{
	Hex = (byte)'h'<<8 | (byte)'x',
	Bech32t = (byte)'b'<<8 | (byte)'t',
}

public enum Algorithm : ushort
{
   Secp256k1 = (byte)'e'<< 8 | (byte)'a',
   Schnorr   = (byte)'e'<< 8 | (byte)'s',
}

public class PublicKey : IComparable, IComparable<PublicKey>, IEquatable<PublicKey>, IBinarySerializable
{
	public const int			Length = 32;
	public virtual byte[]		Bytes { get; protected set; }
	public string				Tag { get; protected set; }
	string						Text;
	KeyFormat					Format = KeyFormat.Bech32t;
	Algorithm	 				Algorithm = Algorithm.Schnorr;

	public PublicKey()
	{
	}

 	public PublicKey(byte[] bytes)
 	{
		if(bytes.Length != Length)
			throw new IntegrityException("Wrong length");
		
		Bytes = bytes;
 	}

 	public PublicKey(byte[] bytes, string tag)
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

 	public PublicKey(KeyFormat format, Algorithm algorithm, byte[] bytes, string tag)
 	{
		Format = format;
		Algorithm = algorithm;
		Bytes = bytes;
		Tag = tag;
 	}

	public void Write(Writer writer)
	{
		writer.Write(Format);
		writer.Write(Algorithm);
		writer.WriteASCII(Tag);
		writer.Write(Bytes);
	}

	public void Read(Reader reader)
	{
		Format = reader.Read<KeyFormat>();
		Algorithm = reader.Read<Algorithm>();
		Tag = reader.ReadASCII();
		Bytes = reader.ReadBytes(Length);
	}

	public static PublicKey Parse(string address)
	{
        if(string.IsNullOrEmpty(address) || address.Length < 6) ///[data>1][taglength=1][format=2][algorithm=2]
            throw new FormatException("Invalid address length");

        var f	= (KeyFormat)(char.ToLowerInvariant(address[address.Length-4]) << 8 | char.ToLowerInvariant(address[address.Length-3]));
        var a	= (Algorithm)	 (char.ToLowerInvariant(address[address.Length-2]) << 8 | char.ToLowerInvariant(address[address.Length-1]));

        if(f != KeyFormat.Bech32t)
            throw new FormatException("Unsupported address format");

        if(a != Algorithm.Schnorr)
            throw new FormatException("Unsupported address format");

        if(!Bech32.TryDecode(address.AsSpan(0, address.Length - 4), out var data, out string tag))
			throw new FormatException("Invalid address format");

		return new PublicKey(data, tag);
	}

	public override string ToString()
	{
		if(!Enum.IsDefined(Format))
			throw new InvalidOperationException();

		if(Text == null)
			Text = Encode(Format, Algorithm.Schnorr, Bytes, Tag);
		
		return  Text;
	}

	public static bool operator == (PublicKey a, PublicKey b)
	{
		return a is null && b is null || a.Equals(b);
	}

	public static bool operator != (PublicKey a, PublicKey b)
	{
		return !(a == b);
	}

	public override bool Equals(object o)
	{
		return o is PublicKey a && Equals(a);  
	}

	public bool Equals(PublicKey a)
	{
		return a is not null && Uccs.Bytes.EqualityComparer.Equals(Bytes, a.Bytes) && Format == a.Format && Algorithm == a.Algorithm;
	}

	public override int GetHashCode()
	{
		return Bytes[0] << 8 & Bytes[1];
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as PublicKey);
	}

	public int CompareTo(PublicKey address)
	{
		return Uccs.Bytes.Comparer.Compare(Bytes, address.Bytes);
	}

    public static string Encode(KeyFormat format, Algorithm algorithm, byte[] data, string tag = null)
    {
        string body = Bech32.Encode(data, tag);

        return $"{body}{(char)((ushort)format >> 8)}{(char)(byte)format}{(char)((ushort)algorithm >> 8)}{(char)(byte)algorithm}";
    }
}

public class PublicKeyJsonConverter : JsonConverter<PublicKey>
{
	public override PublicKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return PublicKey.Parse(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, PublicKey value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
