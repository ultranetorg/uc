using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	/// <summary>
	/// 
	/// ultranet:testzone1
	/// ultranet:testzone1/author
	/// ultranet:testzone1/author/res.our.sce
	/// ultranet:testzone1#0123456789ABCDEF
	/// ultranet:testzone1$author/product:hhhhhhhhhhhhhhhhhhh:sssssssssssssssssssssssssssssssssssssssssssssss
	/// 
	/// </summary>

	public enum ReleaseAddressType
	{
		None, DH, SPD
	}

 	public abstract class ReleaseAddress : ITypeCode, IBinarySerializable, IEquatable<ReleaseAddress>
 	{
 		public abstract byte			TypeCode { get; }
 		public abstract byte[]			MemberOrderKey { get; }
		public string					Zone { get; set; }
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

	//	public const char				S = ':';
	
		public override string ToString()
		{
			return null;
		}

		public static ReleaseAddress Parse(string t)
		{
			var i = t.IndexOf(':');

			var a = t.Substring(0, i) switch{
												"updh" => new DHAddress() as ReleaseAddress,
												"upsd" => new SDAddress(),
												_ => throw new FormatException()
											};

			var z = t.IndexOf('.', i+1);
			
			if(z != -1)
			{	
				a.Zone = t.Substring(i+1, z-i-1);
				a.ParseSpecific(t.Substring(z+1));
			}
			else
				a.ParseSpecific(t.Substring(i+1));

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

		public static ReleaseAddress FromType(byte c)
		{
			switch((ReleaseAddressType)c)
			{
				case ReleaseAddressType.DH:		return new DHAddress();
				case ReleaseAddressType.SPD:	return new SDAddress();
			}

			throw new ResourceException(ResourceError.UnknownAddressType);
		}

		public static ReleaseAddress FromRaw(BinaryReader reader)
		{
			var a = FromType(reader.ReadByte());
			
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
	}
 
 	public class DHAddress : ReleaseAddress
 	{
 		public byte[]			Hash { get; set; }
 		public override byte	TypeCode => (byte)ReleaseAddressType.DH;
 		public override byte[]	MemberOrderKey => Hash;
 		
		public override int		GetHashCode() => BitConverter.ToInt32(Hash);
 		public override bool	Equals(object obj) => Equals(obj as DHAddress);
		public override bool	Equals(ReleaseAddress o) => o is DHAddress a && Hash.SequenceEqual(a.Hash);

		public override string ToString()
		{
			return $"updh:{Zone}{(Zone != null ? "." : null)}{Hash.ToHex()}";
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

	public class SDAddress : ReleaseAddress
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Signature { get; set; }
		public override byte	TypeCode => (byte)ReleaseAddressType.SPD;
		public override byte[]	MemberOrderKey => Signature;

 		public override int		GetHashCode() => BitConverter.ToInt32(Signature);
 		public override bool	Equals(object o) => Equals(o as SDAddress);
		public override bool	Equals(ReleaseAddress o) => o is SDAddress a && Resource == a.Resource && Signature.SequenceEqual(a.Signature);
 
		public override string ToString()
		{
			return $"upsd:{Zone}{(Zone != null ? "." : null)}{Resource.Author}/{Resource.Resource}:{Signature.ToHex()}";
		}
		
		public override void ParseSpecific(string t)
		{
			Resource	= ResourceAddress.ParseAR(t);

			var s = Resource.Resource.LastIndexOf(':');
			
			Resource.Resource = Resource.Resource.Substring(0, s);
			Signature		  = Resource.Resource.Substring(s + 1).FromHex();
		}

		public bool Prove(Cryptography cryptography, AccountAddress account, byte[] hash)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			w.Write(Resource);
			w.Write(hash);

			return cryptography.AccountFrom(Signature, cryptography.Hash(s.ToArray())) == account;
		}

		public static ReleaseAddress Create(Cryptography cryptography, AccountKey key, ResourceAddress resource, byte[] hash)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			w.Write(resource);
			w.Write(hash);

			return new SDAddress {Resource = resource, Signature = cryptography.Sign(key, cryptography.Hash(s.ToArray()))};
		}

		protected override void WriteMore(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write(Signature);
		}

		protected override void ReadMore(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
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

	public class ReleaseAddressCreator
	{
		public ReleaseAddressType	Type { get; set; }
		public AccountAddress		Owner { get; set; }
		public ResourceAddress		Resource { get; set; }

		public ReleaseAddress Create(Sun sun, byte[] hash)
		{
			return Type	switch
						{
							ReleaseAddressType.DH => new DHAddress {Hash = hash},
							ReleaseAddressType.SPD => SDAddress.Create(sun.Zone.Cryptography, sun.Vault.GetKey(Owner), Resource, hash),
							_ => throw new ResourceException(ResourceError.UnknownAddressType)
						};

		}
	}
}
