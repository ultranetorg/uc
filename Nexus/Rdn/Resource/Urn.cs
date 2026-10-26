using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

/// <summary>
/// 
/// </summary>

public enum UrnNamespace : uint
{
	None, Blake3
}

public class Urn : EntityId, IBinarySerializable, IEquatable<Urn>, ITextSerialisable
{
	public UrnNamespace					Namespace { get; set; }
	public byte[]						Hash { get; set; }
 	public byte[]						MemberOrderKey => Hash;
	public override int					Bucket => BitConverter.ToInt32(Hash);
	
	public override int					GetHashCode() => BitConverter.ToInt32(Hash);
 	public override bool				Equals(object obj) => Equals(obj as Urn);
	public bool							Equals(Urn a) => a is not null && Namespace == a.Namespace && Bytes.Equal(Hash, a.Hash);

	static Urn()
	{
	}

	public Urn()
	{
	}

	public static Urn Blake3(byte[] hash)
	{
		var u = new Urn();
		u.Namespace = UrnNamespace.Blake3;
		u.Hash = hash;

		return u;
	}

	public void Read(string text)
	{
		Parse(text);
	}

	public override string ToString()
	{
		if(Namespace == UrnNamespace.None)
			throw new InvalidOperationException("Namespace not defined");

		return $"urn:{Namespace.ToString().ToLowerInvariant()}:{Hash.ToHex()}";
	}

	public static Urn Parse(string t)
	{
		Snq.Parse(t, out var s, out var n, out var _);

		var i = n.IndexOf(':');

		if(i == -1)
			throw new FormatException();

 		var u = new Urn();
		u.Namespace = Enum.Parse<UrnNamespace>(n.AsSpan(0, i), true);

 		switch(u.Namespace)
 		{
 			case UrnNamespace.Blake3 :
 			{
 				var h = n.AsSpan(i + 1);
 				
 				if(h.Length/2 != Cryptography.HashLength)
 					throw new FormatException();
 				
 				u.Hash = h.FromHex();
 
 				return u;
 			}

 			default:
				throw new FormatException();
 		};
	}

	protected void WriteMore(Writer writer)
	{
 		writer.Write(Hash);
	}

	protected void ReadMore(Reader reader)
	{
 		Hash = reader.ReadBytes(Cryptography.HashLength);
	}

	public override void Write(Writer writer)
	{
		writer.Write(Namespace);
		WriteMore(writer);
	}

	public override void Read(Reader reader)
	{
		Namespace = reader.Read<UrnNamespace>();
		ReadMore(reader);
	}

 	public static Urn FromRaw(byte[] bytes)
 	{
 		using var r = new Reader(bytes);
 
		var u = new Urn();
 		u.Read(r);

		return u;
 	}
 
 	public static bool operator == (Urn a, Urn b)
 	{
 		return a is null && b is null || a is not null && a.Equals(b);
 	}
 
 	public static bool operator != (Urn a, Urn b)
 	{
 		return !(a == b);
 	}

	public override bool Equals(EntityId other)
	{
		return Equals((Urn)other);
	}

	public override int CompareTo(EntityId other)
	{
		return Bytes.Comparer.Compare(Hash, (other as Urn).Hash);
	}

	public bool Verify(byte[] hash)
	{
		return Hash.SequenceEqual(hash);
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
	public UrnNamespace		Type { get; set; }
	public PublicKey		Owner { get; set; }
	public Ura				Resource { get; set; }

	public ReleaseAddressCreator(UrnNamespace type)
	{
		Type = type;
	}

	public Urn Create(VaultApiClient vault, byte[] hash)
	{
		return Type	switch
					{
						UrnNamespace.Blake3 => Urn.Blake3(hash),
						///UrrScheme.Urrsd => Urrsd.Create(vault.Cryptography, vault.Find(Owner).Key, Resource, hash),
						_ => throw new ResourceException(ResourceError.UnknownAddressType)
					};
	}
}
