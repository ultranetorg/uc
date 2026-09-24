using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

/// <summary>
/// 
/// </summary>

public enum UrnScheme : uint
{
	None, Hcid
}

public abstract class Urn : ITypeCode, IBinarySerializable, IEquatable<Urn>, ITextSerialisable
{
 	public abstract byte[]			MemberOrderKey { get; }
 	public byte[]					Raw => _Raw ??= (this as IBinarySerializable).ToRaw();
	byte[]							_Raw;

	public abstract UrnScheme		Scheme { get; }
	public override abstract bool	Equals(object other);
  	public abstract bool			Equals(Urn other);
	public override abstract int	GetHashCode();
	public abstract void			ParseSpecific(string t);
	public override abstract string ToString();

	//	public const char				S = ':';

	static Urn()
	{
	}


	public void Read(string text)
	{
		Parse(text);
	}

	public static Urn Parse(string t)
	{
		Snq.Parse(t, out var s, out var z, out var o);

		var a = Enum.Parse<UrnScheme>(s, true)	switch
												{
													UrnScheme.Hcid => new Hcid() as Urn,
													//UrrScheme.Urrsd => new Urrsd(),
													_ => throw new FormatException()
												};

		a.ParseSpecific(o);

		return a;
	}

	
	protected virtual void WriteMore(Writer writer)
	{
	}

	protected virtual void ReadMore(Reader reader)
	{
	}

	public virtual void Write(Writer writer)
	{
		WriteMore(writer);
	}

	public virtual void Read(Reader reader)
	{
		ReadMore(reader);
	}

	public void WriteVirtual(Writer writer)
	{
		writer.Write(Scheme);
		WriteMore(writer);
	}

	public static Urn ReadVirtual(Reader reader)
	{
		var a = reader.Read<UrnScheme>() switch
										 {
										 	UrnScheme.Hcid => new Hcid() as Urn,
										 	//UrrScheme.Urrsd => new Urrsd(),
										 	_ => throw new FormatException()
										 };
		
		a.ReadMore(reader);
		
		return a;
	}

 	public static Urn FromRaw(byte[] bytes)
 	{
 		using var r = new Reader(bytes);
 
 		return ReadVirtual(r);
 	}
 
 	public static bool operator == (Urn a, Urn b)
 	{
 		return a is null && b is null || a is not null && a.Equals(b);
 	}
 
 	public static bool operator != (Urn a, Urn b)
 	{
 		return !(a == b);
 	}
}
 
public class Hcid : Urn /// Rdn Resource Release Hash
{
	public override UrnScheme	Scheme => UrnScheme.Hcid; 

	public Hcid()
	{
	}

	public Hcid(byte[] hash)
	{
		Hash = hash;
	}

	public byte[]			Hash { get; set; }
 	public override byte[]	MemberOrderKey => Hash;
 		
	public override int		GetHashCode() => BitConverter.ToInt32(Hash);
 	public override bool	Equals(object obj) => Equals(obj as Hcid);
	public override bool	Equals(Urn o) => o is Hcid a && Hash.SequenceEqual(a.Hash);

	public new static Hcid Parse(string t)
	{
		Snq.Parse(t, out var s, out var z, out var o);

		var a = new Hcid();

		a.ParseSpecific(o);

		return a;
	}
	public override string ToString()
	{
		return Snq.ToString(Scheme.ToString(), null, Hash.ToHex());
	}

	public override void ParseSpecific(string t)
	{
		Hash = t.FromHex();
	}

	public bool Verify(byte[] hash)
	{
		return Hash.SequenceEqual(hash);
	}

	protected override void WriteMore(Writer writer)
	{
 		writer.Write(Hash);
	}

	protected override void ReadMore(Reader reader)
	{
 		Hash = reader.ReadBytes(Cryptography.HashLength);
	}
}

public class UrrJsonConverter : JsonConverter<Urn>
{
	public override Urn Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Urn.Parse(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, Urn value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}

public class ReleaseAddressCreator
{
	public UrnScheme		Type { get; set; }
	public PublicKey		Owner { get; set; }
	public Ura				Resource { get; set; }

	public Urn Create(VaultApiClient vault, byte[] hash)
	{
		return Type	switch
					{
						UrnScheme.Hcid => new Hcid {Hash = hash},
						///UrrScheme.Urrsd => Urrsd.Create(vault.Cryptography, vault.Find(Owner).Key, Resource, hash),
						_ => throw new ResourceException(ResourceError.UnknownAddressType)
					};
	}
}
