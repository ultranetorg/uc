using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public enum ReleaseAddressType
	{
		None, DHA, SPD
	}

 	public abstract class ReleaseAddress : ITypeCode, IBinarySerializable, IEquatable<ReleaseAddress>
 	{
 		public abstract byte			TypeCode { get; }
 		public byte[]					Hash { get; set; }
 		public byte[]					Raw {
												get
												{
													var s = new MemoryStream();
													var w = new BinaryWriter(s);
													
 													w.Write((byte)TypeCode);
													Write(w);
													
													return s.ToArray();
												}
											}

		public override abstract bool	Equals(object other);
  		public abstract bool			Equals(ReleaseAddress other);

		public static ReleaseAddress Parse(string t)
		{
			var i = t.IndexOf('#');

			var s = new MemoryStream(t.Substring(i + 1).FromHex());
			var r = new BinaryReader(s);

			switch(Enum.Parse<ReleaseAddressType>(t.Substring(0, i).ToUpper()))
			{
				case ReleaseAddressType.DHA: 
				{
					var a = new DHAAddress();
					a.Read(r);
					return a;
				}

				case ReleaseAddressType.SPD: 
				{
					var a = new SPDAddress();
					a.Read(r);
					return a;
				}

				default:
					throw new IntegrityException();
			}
		}
		
		protected virtual void WriteMore(BinaryWriter writer)
		{
		}

		protected virtual void ReadMore(BinaryReader reader)
		{
		}

		public virtual void Write(BinaryWriter writer)
		{
 			writer.Write(Hash);
			WriteMore(writer);
		}

		public virtual void Read(BinaryReader reader)
		{
 			Hash = reader.ReadBytes(Cryptography.HashSize);
			ReadMore(reader);
		}

		public static ReleaseAddress FromType(byte c)
		{
			switch((ReleaseAddressType)c)
			{
				case ReleaseAddressType.DHA:	return new DHAAddress();
				case ReleaseAddressType.SPD:	return new SPDAddress();
			}

			throw new ResourceException(ResourceError.UnknownAddressType);
		}

		public static ReleaseAddress FromRaw(BinaryReader reader)
		{
			var a = FromType(reader.ReadByte());
			
 			a.Hash = reader.ReadBytes(Cryptography.HashSize);
			a.ReadMore(reader);
			
			return a;
		}

 		public static ReleaseAddress FromRaw(byte[] bytes)
 		{
 			var s = new MemoryStream(bytes);
 			var r = new BinaryReader(s);
 
 			return FromRaw(r);
 		}
 
 		public static bool operator == (ReleaseAddress a, ReleaseAddress b)
 		{
 			return a is null && b is null || a is not null && a.Equals(b);
 		}
 
 		public static bool operator != (ReleaseAddress a, ReleaseAddress b)
 		{
 			return !(a == b);
 		}
 
 		public override int GetHashCode()
 		{
 			return BitConverter.ToInt32(Hash);
 		}
	}

	public class ReleaseAddressCreator
	{
		public ReleaseAddressType	Type { get; set; }
		public AccountAddress		Owner { get; set; }
		public ResourceAddress		Resource { get; set; }

		public ReleaseAddress Create(Sun sun, byte[] hash)
		{
			return Type	switch
						{
							ReleaseAddressType.DHA => new DHAAddress {Hash = hash},
							ReleaseAddressType.SPD => SPDAddress.Create(sun.Zone.Cryptography, sun.Vault.GetKey(Owner), Resource, hash),
							_ => throw new ResourceException(ResourceError.UnknownAddressType)
						};

		}
	}
 
 	public class DHAAddress : ReleaseAddress
 	{
 		public override byte	TypeCode => (byte)ReleaseAddressType.DHA;
 		
		public override int		GetHashCode() => base.GetHashCode();
 		public override bool	Equals(object obj) => Equals(obj as DHAAddress);
		public override bool	Equals(ReleaseAddress o) => o is DHAAddress a && Hash.SequenceEqual(o.Hash);

		public override string ToString()
		{
			return $"dha#{Hash.ToHex()}";
		}

		public bool Verify(byte[] hash)
		{
			return Hash.SequenceEqual(hash);
		}
 	}

	public class SPDAddress : ReleaseAddress
	{
		public byte[]			Signature { get; set; }
		public override byte	TypeCode => (byte)ReleaseAddressType.SPD;

 		public override int		GetHashCode() => base.GetHashCode();
 		public override bool	Equals(object o) => Equals(o as SPDAddress);
		public override bool	Equals(ReleaseAddress o) => o is SPDAddress a && Hash.SequenceEqual(o.Hash) && Signature.SequenceEqual(a.Signature);
 
		public override string ToString()
		{
			return $"spd#{Hash.ToHex()}{Signature.ToHex()}";
		}

		public bool Valid(Cryptography cryptography, ResourceAddress resource, AccountAddress account)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			w.Write(resource);
			w.Write(Hash);

			return cryptography.AccountFrom(Signature, cryptography.Hash(s.ToArray())) == account;
		}

		public static ReleaseAddress Create(Cryptography cryptography, AccountKey key, ResourceAddress resource, byte[] hash)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			w.Write(resource);
			w.Write(hash);

			return new SPDAddress {Hash = hash, Signature = cryptography.Sign(key, cryptography.Hash(s.ToArray()))};
		}

		protected override void WriteMore(BinaryWriter writer)
		{
			writer.Write(Signature);
		}

		protected override void ReadMore(BinaryReader reader)
		{
			Signature = reader.ReadBytes(Cryptography.SignatureSize);
		}
	}

	public class ReleaseAddressJsonConverter : JsonConverter<ReleaseAddress>
	{
		public override ReleaseAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ReleaseAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, ReleaseAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
