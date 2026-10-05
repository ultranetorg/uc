using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

/// <summary>
/// 
/// </summary>

public enum UrnNid : uint
{
	None, Blake3
}

public abstract class Urn : ITypeCode, IBinarySerializable, IEquatable<Urn>, ITextSerialisable
{
	public abstract UrnNid				Nid { get; }
 	public abstract byte[]				MemberOrderKey { get; }
 	public byte[]						Raw => _Raw ??= (this as IBinarySerializable).ToRaw();
	byte[]								_Raw;

	public override abstract bool		Equals(object other);
  	public abstract bool				Equals(Urn other);
	public override abstract int		GetHashCode();
	public abstract void				ParseSpecific(ReadOnlySpan<char> t);
	public override abstract string		ToString();

	static Urn()
	{
	}

	public void Read(string text)
	{
		Parse(text);
	}

	public static Urn Parse(string t)
	{
		Snq.Parse(t, out var s, out var n, out var _);

		var i = n.IndexOf(':');

		if(i == -1)
			throw new FormatException();

		var a = Enum.Parse<UrnNid>(n.AsSpan(0, i),  true)	switch
															{
																UrnNid.Blake3 => new Blake3rn() as Urn,
																//UrrScheme.Urrsd => new Urrsd(),
																_ => throw new FormatException()
															};

		a.ParseSpecific(n.AsSpan(i + 1));

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
		writer.Write(Nid);
		WriteMore(writer);
	}

	public static Urn ReadVirtual(Reader reader)
	{
		var a = reader.Read<UrnNid>()	switch
										{
											UrnNid.Blake3 => new Blake3rn() as Urn,
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
 
public class Blake3rn : Urn /// Rdn Resource Release Hash
{
	public override UrnNid	Nid => UrnNid.Blake3; 

	public byte[]			Hash { get; set; }
 	public override byte[]	MemberOrderKey => Hash;
 		
	public override int		GetHashCode() => BitConverter.ToInt32(Hash);
 	public override bool	Equals(object obj) => Equals(obj as Blake3rn);
	public override bool	Equals(Urn o) => o is Blake3rn a && Bytes.Equal(Hash, a.Hash);

	public Blake3rn()
	{
	}

	public Blake3rn(byte[] hash)
	{
		Hash = hash;
	}

	public override string ToString()
	{
		return $"urn:{Nid}:{Hash.ToHex()}";
	}

	public override void ParseSpecific(ReadOnlySpan<char> t)
	{
		if(t.Length/2 != Cryptography.HashLength)
			throw new FormatException();

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
	public UrnNid		Type { get; set; }
	public PublicKey		Owner { get; set; }
	public Ura				Resource { get; set; }

	public Urn Create(VaultApiClient vault, byte[] hash)
	{
		return Type	switch
					{
						UrnNid.Blake3 => new Blake3rn {Hash = hash},
						///UrrScheme.Urrsd => Urrsd.Create(vault.Cryptography, vault.Find(Owner).Key, Resource, hash),
						_ => throw new ResourceException(ResourceError.UnknownAddressType)
					};
	}
}
