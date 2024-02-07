using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public enum ReleaseAddressType
	{
		None, Hash, Proving
	}

	[JsonDerivedType(typeof(HashAddress), typeDiscriminator: "Hash")]
	[JsonDerivedType(typeof(ProvingAddress), typeDiscriminator: "Proving")]
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

		public override string ToString()
		{
			return Raw.ToHex();
		}

		public static ReleaseAddress Parse(string t)
		{
			return FromRaw(t.FromHex());
		}
		
		protected virtual void WriteMore(BinaryWriter writer)
		{
		}

		protected virtual void ReadMore(BinaryReader reader)
		{
		}

		public virtual void Write(BinaryWriter writer)
		{
 			writer.WriteBytes(Hash);
			WriteMore(writer);
		}

		public virtual void Read(BinaryReader reader)
		{
 			Hash = reader.ReadBytes();
			ReadMore(reader);
		}

		public static ReleaseAddress FromType(byte c)
		{
			switch((ReleaseAddressType)c)
			{
				case ReleaseAddressType.Hash:		return new HashAddress();
				case ReleaseAddressType.Proving:	return new ProvingAddress();
			}

			throw new ResourceException(ResourceError.UnknownAddressType);
		}

		public static ReleaseAddress FromRaw(BinaryReader reader)
		{
			var a = FromType(reader.ReadByte());
			
 			a.Hash = reader.ReadBytes();
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
							ReleaseAddressType.Hash => new HashAddress {Hash = hash},
							ReleaseAddressType.Proving => ProvingAddress.Create(sun.Zone.Cryptography, sun.Vault.GetKey(Owner), Resource, hash),
							_ => throw new ResourceException(ResourceError.UnknownAddressType)
						};

		}
	}
 
 	public class HashAddress : ReleaseAddress
 	{
 		public override byte	TypeCode => (byte)ReleaseAddressType.Hash;
 		
		public override int		GetHashCode() => base.GetHashCode();
 		public override bool	Equals(object obj) => Equals(obj as HashAddress);
		public override bool	Equals(ReleaseAddress o) => o is HashAddress a && Hash.SequenceEqual(o.Hash);

		public bool Verify(byte[] hash)
		{
			return Hash.SequenceEqual(hash);
		}
 	}

	public class ProvingAddress : ReleaseAddress
	{
		public byte[]			Signature { get; set; }
		public override byte	TypeCode => (byte)ReleaseAddressType.Proving;

 		public override int		GetHashCode() => base.GetHashCode();
 		public override bool	Equals(object o) => Equals(o as ProvingAddress);
		public override bool	Equals(ReleaseAddress o) => o is ProvingAddress a && Hash.SequenceEqual(o.Hash) && Signature.SequenceEqual(a.Signature);
 
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

			return new ProvingAddress {Hash = hash, Signature = cryptography.Sign(key, cryptography.Hash(s.ToArray()))};
		}

		protected override void WriteMore(BinaryWriter writer)
		{
			writer.WriteBytes(Signature);
		}

		protected override void ReadMore(BinaryReader reader)
		{
			Signature = reader.ReadBytes();
		}
	}
}
