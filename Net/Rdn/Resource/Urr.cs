using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	/// <summary>
	/// 
	/// </summary>

	public enum UrrScheme
	{
		None, Urrh, Urrs
	}

 	public abstract class Urr : ITypeCode, IBinarySerializable, IEquatable<Urr>, ITextSerialisable
 	{
 		public abstract byte			TypeCode { get; }
 		public abstract byte[]			MemberOrderKey { get; }
		public string					Zone { get; set; }
 		public byte[]					Raw {
												get
												{
													var s = new MemoryStream();
													var w = new BinaryWriter(s);
													
 													w.Write(TypeCode);
													Write(w);
													
													return s.ToArray();
												}
											}

		public override abstract bool	Equals(object other);
  		public abstract bool			Equals(Urr other);
		public override abstract int	GetHashCode();

		//	public const char				S = ':';

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
			UAddress.Parse(t, out var s, out var z, out var o);

			var a = s switch{
								Urrh.Scheme =>	new Urrh() as Urr,
								Urrsd.Scheme => new Urrsd(),
								_ => throw new FormatException()
							};

			a.Zone = z;
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

		public static Urr FromType(byte c)
		{
			switch((UrrScheme)c)
			{
				case UrrScheme.Urrh: return new Urrh();
				case UrrScheme.Urrs: return new Urrsd();
			}

			throw new ResourceException(ResourceError.UnknownAddressType);
		}

		public static Urr FromRaw(BinaryReader reader)
		{
			var a = FromType(reader.ReadByte());
			
			a.ReadMore(reader);
			
			return a;
		}

 		public static Urr FromRaw(byte[] bytes)
 		{
 			var s = new MemoryStream(bytes);
 			var r = new BinaryReader(s);
 
 			return FromRaw(r);
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
		public const string		Scheme = "urrh";

 		public byte[]			Hash { get; set; }
 		public override byte	TypeCode => (byte)UrrScheme.Urrh;
 		public override byte[]	MemberOrderKey => Hash;
 		
		public override int		GetHashCode() => BitConverter.ToInt32(Hash);
 		public override bool	Equals(object obj) => Equals(obj as Urrh);
		public override bool	Equals(Urr o) => o is Urrh a && Hash.SequenceEqual(a.Hash);

		public override string ToString()
		{
			return UAddress.ToString(Scheme, Zone, Hash.ToHex());
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
		public const string		Scheme = "urrsd";

		public Ura				Resource { get; set; }
		public byte[]			Signature { get; set; }
		public override byte	TypeCode => (byte)UrrScheme.Urrs;
		public override byte[]	MemberOrderKey => Signature;

 		public override int		GetHashCode() => BitConverter.ToInt32(Signature);
 		public override bool	Equals(object o) => Equals(o as Urrsd);
		public override bool	Equals(Urr o) => o is Urrsd a && Resource == a.Resource && Signature.SequenceEqual(a.Signature);
 
		public override string ToString()
		{
			return UAddress.ToString(Scheme, Zone, $"{Resource.Domain}/{Resource.Resource}:{Signature.ToHex()}");
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

			return cryptography.AccountFrom(Signature, cryptography.Hash(s.ToArray())) == account;
		}

		public static Urr Create(Cryptography cryptography, AccountKey key, Ura resource, byte[] hash)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			w.Write(resource);
			w.Write(hash);

			return new Urrsd {Resource = resource, Signature = cryptography.Sign(key, cryptography.Hash(s.ToArray()))};
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

	public class ReleaseAddressJsonConverter : JsonConverter<Urr>
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

		public Urr Create(Rdn sun, byte[] hash)
		{
			return Type	switch
						{
							UrrScheme.Urrh => new Urrh {Hash = hash},
							UrrScheme.Urrs => Urrsd.Create(sun.Zone.Cryptography, sun.Node.Vault.GetKey(Owner), Resource, hash),
							_ => throw new ResourceException(ResourceError.UnknownAddressType)
						};

		}
	}
}
