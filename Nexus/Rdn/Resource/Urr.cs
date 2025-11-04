using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn;

/// <summary>
/// 
/// </summary>

public enum UrrScheme
{
	None, Urrh, Urrsd
}

public abstract class Urr : ITypeCode, IBinarySerializable, IEquatable<Urr>, ITextSerialisable
{
 	public abstract byte[]			MemberOrderKey { get; }
	public string					Net { get; set; }
 	public byte[]					Raw {
										get
										{
											var s = new MemoryStream();
											var w = new BinaryWriter(s);
												
 												w.Write((byte)Scheme);
											Write(w);
												
											return s.ToArray();
										}
									}

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
		Unel.Parse(t, out var s, out var z, out var o);

		var a = Enum.Parse<UrrScheme>(s, true)	switch
												{
													UrrScheme.Urrh => new Urrh() as Urr,
													UrrScheme.Urrsd => new Urrsd(),
													_ => throw new FormatException()
												};

		a.Net = z;
		a.ParseSpecific(o);

		return a;
	}

	public abstract void ParseSpecific(string t);
	
	protected virtual void WriteMore(BinaryWriter writer)
	{
	}

	protected virtual void ReadMore(BinaryReader reader)
	{
	}

	public virtual void Write(BinaryWriter writer)
	{
		WriteMore(writer);
	}

	public virtual void Read(BinaryReader reader)
	{
		ReadMore(reader);
	}

	public void WriteVirtual(BinaryWriter writer)
	{
		writer.Write((byte)Scheme);
		WriteMore(writer);
	}

	public static Urr ReadVirtual(BinaryReader reader)
	{
		var a = (UrrScheme)reader.ReadByte()switch
											{
												UrrScheme.Urrh => new Urrh() as Urr,
												UrrScheme.Urrsd => new Urrsd(),
												_ => throw new FormatException()
											};
		
		a.ReadMore(reader);
		
		return a;
	}

 	public static Urr FromRaw(byte[] bytes)
 	{
 		var s = new MemoryStream(bytes);
 		var r = new BinaryReader(s);
 
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
 
public class Urrh : Urr
{
	public override UrrScheme	Scheme => UrrScheme.Urrh;

	public Urrh()
	{
	}

	public Urrh(byte[] hash)
	{
		Hash = hash;
	}

	public byte[]			Hash { get; set; }
 	public override byte[]	MemberOrderKey => Hash;
 		
	public override int		GetHashCode() => BitConverter.ToInt32(Hash);
 	public override bool	Equals(object obj) => Equals(obj as Urrh);
	public override bool	Equals(Urr o) => o is Urrh a && Hash.SequenceEqual(a.Hash);

	public new static Urrh Parse(string t)
	{
		Unel.Parse(t, out var s, out var z, out var o);

		var a = new Urrh();

		a.Net = z;
		a.ParseSpecific(o);

		return a;
	}
	public override string ToString()
	{
		return Unel.ToString(Scheme.ToString(), Net, Hash.ToHex());
	}

	public override void ParseSpecific(string t)
	{
		Hash = t.FromHex();
	}

	public bool Verify(byte[] hash)
	{
		return Hash.SequenceEqual(hash);
	}

	protected override void WriteMore(BinaryWriter writer)
	{
 		writer.Write(Hash);
	}

	protected override void ReadMore(BinaryReader reader)
	{
 		Hash = reader.ReadBytes(Cryptography.HashSize);
	}
  }

public class Urrsd : Urr
{
	public override UrrScheme	Scheme => UrrScheme.Urrsd;

	public Ura					Resource { get; set; }
	public byte[]				Signature { get; set; }
	public override byte[]		MemberOrderKey => Signature;

 	public override int			GetHashCode() => BitConverter.ToInt32(Signature);
 	public override bool		Equals(object o) => Equals(o as Urrsd);
	public override bool		Equals(Urr o) => o is Urrsd a && Resource == a.Resource && Signature.SequenceEqual(a.Signature);
 
	public override string ToString()
	{
		return Unel.ToString(Scheme.ToString(), Net, $"{Resource.Domain}/{Resource.Resource}:{Signature.ToHex()}");
	}
	
	public override void ParseSpecific(string t)
	{
		Resource	= Ura.ParseAR(t);

		var s = Resource.Resource.LastIndexOf(':');
		
		Signature		  = Resource.Resource.Substring(s + 1).FromHex();
		Resource.Resource = Resource.Resource.Substring(0, s);
	}

	public bool Prove(Cryptography cryptography, AccountAddress account, byte[] hash)
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);
		w.Write(Resource);
		w.Write(hash);

		return cryptography.AccountFrom(Signature, Cryptography.Hash(s.ToArray())) == account;
	}

	public static Urr Create(Cryptography cryptography, AccountKey key, Ura resource, byte[] hash)
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);
		w.Write(resource);
		w.Write(hash);

		return new Urrsd {Resource = resource, Signature = cryptography.Sign(key, Cryptography.Hash(s.ToArray()))};
	}

	protected override void WriteMore(BinaryWriter writer)
	{
		writer.Write(Resource);
		writer.Write(Signature);
	}

	protected override void ReadMore(BinaryReader reader)
	{
		Resource = reader.Read<Ura>();
		Signature = reader.ReadBytes(Cryptography.SignatureSize);
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
	public AccountAddress	Owner { get; set; }
	public Ura				Resource { get; set; }

	public Urr Create(VaultApiClient vault, byte[] hash)
	{
		return Type	switch
					{
						UrrScheme.Urrh => new Urrh {Hash = hash},
						///UrrScheme.Urrsd => Urrsd.Create(vault.Cryptography, vault.Find(Owner).Key, Resource, hash),
						_ => throw new ResourceException(ResourceError.UnknownAddressType)
					};
	}
}
