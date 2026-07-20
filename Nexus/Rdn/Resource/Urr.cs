using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

/// <summary>
/// 
/// </summary>

public enum UrrScheme : uint
{
	None, Rrrh, Rrrsd
}

public abstract class Urr : ITypeCode, IBinarySerializable, IEquatable<Urr>, ITextSerialisable
{
 	public abstract byte[]			MemberOrderKey { get; }
	public string					Net { get; set; }
 	public byte[]					Raw => _Raw ??= (this as IBinarySerializable).ToRaw();
	byte[]							_Raw;

	public abstract UrrScheme		Scheme { get; }
	public override abstract bool	Equals(object other);
  	public abstract bool			Equals(Urr other);
	public override abstract int	GetHashCode();

	//	public const char				S = ':';

	static Urr()
	{
	}

	public override string ToString()
	{
		return null;
	}

	public void Read(string text)
	{
		Parse(text);
	}

	public static Urr Parse(string t)
	{
		Snq.Parse(t, out var s, out var z, out var o);

		var a = Enum.Parse<UrrScheme>(s, true)	switch
												{
													UrrScheme.Rrrh => new Rrrh() as Urr,
													//UrrScheme.Urrsd => new Urrsd(),
													_ => throw new FormatException()
												};

		a.Net = z;
		a.ParseSpecific(o);

		return a;
	}

	public abstract void ParseSpecific(string t);
	
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

	public static Urr ReadVirtual(Reader reader)
	{
		var a = reader.Read<UrrScheme>() switch
										 {
										 	UrrScheme.Rrrh => new Rrrh() as Urr,
										 	//UrrScheme.Urrsd => new Urrsd(),
										 	_ => throw new FormatException()
										 };
		
		a.ReadMore(reader);
		
		return a;
	}

 	public static Urr FromRaw(byte[] bytes)
 	{
 		var r = new Reader(bytes);
 
 		return ReadVirtual(r);
 	}
 
 	public static bool operator == (Urr a, Urr b)
 	{
 		return a is null && b is null || a is not null && a.Equals(b);
 	}
 
 	public static bool operator != (Urr a, Urr b)
 	{
 		return !(a == b);
 	}
}
 
public class Rrrh : Urr /// Rdn Resource Release Hash
{
	public override UrrScheme	Scheme => UrrScheme.Rrrh; 

	public Rrrh()
	{
	}

	public Rrrh(byte[] hash)
	{
		Hash = hash;
	}

	public byte[]			Hash { get; set; }
 	public override byte[]	MemberOrderKey => Hash;
 		
	public override int		GetHashCode() => BitConverter.ToInt32(Hash);
 	public override bool	Equals(object obj) => Equals(obj as Rrrh);
	public override bool	Equals(Urr o) => o is Rrrh a && Hash.SequenceEqual(a.Hash);

	public new static Rrrh Parse(string t)
	{
		Snq.Parse(t, out var s, out var z, out var o);

		var a = new Rrrh();

		a.Net = z;
		a.ParseSpecific(o);

		return a;
	}
	public override string ToString()
	{
		return Snq.ToString(Scheme.ToString(), Net, Hash.ToHex());
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

public class UrrJsonConverter : JsonConverter<Urr>
{
	public override Urr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Urr.Parse(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, Urr value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}

public class ReleaseAddressCreator
{
	public UrrScheme		Type { get; set; }
	public PublicKey	Owner { get; set; }
	public Ura				Resource { get; set; }

	public Urr Create(VaultApiClient vault, byte[] hash)
	{
		return Type	switch
					{
						UrrScheme.Rrrh => new Rrrh {Hash = hash},
						///UrrScheme.Urrsd => Urrsd.Create(vault.Cryptography, vault.Find(Owner).Key, Resource, hash),
						_ => throw new ResourceException(ResourceError.UnknownAddressType)
					};
	}
}
