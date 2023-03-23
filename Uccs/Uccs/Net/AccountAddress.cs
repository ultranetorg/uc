using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Linq;
using System.Text;
using Nethereum.Hex.HexConvertors.Extensions;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace UC.Net
{
	public class AccountAddress : IComparable, IComparable<AccountAddress>, IEquatable<AccountAddress>, IBinarySerializable
	{
		public const int		Length = 20;
		protected byte[]		Bytes;
		public static readonly	AccountAddress Zero = new AccountAddress(new byte[Length]);
		public byte[]			Prefix => Bytes.Take(Consensus.PrefixLength).ToArray();

		public static implicit operator byte[] (AccountAddress d) => d.Bytes;
		
		public byte	this[int k] => Bytes[k];
 
		public AccountAddress()
		{
		}

		public AccountAddress(EthECKey k)
		{
			var initaddr = Sha3Keccack.Current.CalculateHash(k.GetPubKeyNoPrefix());
			Bytes = new byte[initaddr.Length - 12];
			Array.Copy(initaddr, 12, Bytes, 0, initaddr.Length - 12);

			if(Bytes.Length != Length)
				throw new IntegrityException("Bytes.Length != Length");
		}

 		public AccountAddress(byte[] b)
 		{
			if(b.Length == Length)
				Bytes = b;
			else
				throw new IntegrityException("Wrong length");
 		}

		public void Write(BinaryWriter w)
		{
			w.Write(Bytes);
		}

		public void Read(BinaryReader r)
		{
			Bytes = r.ReadBytes(Length);
		}

		public static AccountAddress Parse(string pubaddr)
		{
			return new AccountAddress(AddressUtil.Current.ConvertToValid20ByteAddress(pubaddr).HexToByteArray());
		}

		public override string ToString()
		{
			return Bytes != null ? "0x" + Bytes.ToHex() : "";
		}

		public static bool operator == (AccountAddress a, AccountAddress b)
		{
			var x = a as object;
			var y = b as object;

			return x == y || (x == null && y == null) || (x != null && y != null && a.Bytes.SequenceEqual(b.Bytes));
		}

		public static bool operator != (AccountAddress a, AccountAddress b)
		{
			return !(a == b);
		}

		public override bool Equals(object o)
		{
			if(o is AccountAddress)
				return Equals((AccountAddress)o);

			return false; 
		}

		public bool Equals(AccountAddress a)
		{
			return this == a;
		}

		public override int GetHashCode()
		{
			return Bytes[0].GetHashCode() ^ Bytes[1].GetHashCode();
		}

		public int CompareTo(object obj)
		{
			var x = Bytes;
			var y = ((AccountAddress)obj).Bytes;

			var len = Math.Min(x.Length, y.Length);

			for (var i = 0; i < len; i++)
			{
			    var c = x[i].CompareTo(y[i]);
			    if (c != 0)
			    {
			        return c;
			    }
			}

			return x.Length.CompareTo(y.Length);
		}

		public int CompareTo([AllowNull] AccountAddress other)
		{
			return CompareTo(other as object);
		}
	}
	
	public class AccountKey : AccountAddress
	{
		[JsonIgnore]
		public EthECKey		Key { get; protected set; }

		public AccountKey(EthECKey k)
		{
			Key = k;

			var initaddr = Sha3Keccack.Current.CalculateHash(k.GetPubKeyNoPrefix());
			Bytes = new byte[initaddr.Length - 12];
			Array.Copy(initaddr, 12, Bytes, 0, initaddr.Length - 12);

			if(Bytes.Length != Length)
				throw new IntegrityException("Bytes.Length != Length");
		}
	
		public static AccountKey Create()
		{
			return new AccountKey(EthECKey.GenerateKey());
		}

		public new static AccountKey Parse(string privatekay)
		{
			return new AccountKey(new EthECKey(privatekay));
		}

		public static AccountKey Load(Cryptography cryptography, string path, string password)
		{
			return new AccountKey(new EthECKey(cryptography.Decrypt(File.ReadAllBytes(path), password), true));
		}

		public static AccountKey Load(string path, string password)
		{
			return new AccountKey(new EthECKey(Cryptography.Current.Decrypt(File.ReadAllBytes(path), password), true));
		}

		public static AccountKey Load(byte[] wallet, string password)
		{
			return new AccountKey(new EthECKey(Cryptography.Current.Decrypt(wallet, password), true));
		}

		public void Save(string path, string password)
		{
			File.WriteAllBytes(path, Cryptography.Current.Encrypt(Key, password));
		}

		public byte[] Save(string password)
		{
			return Cryptography.Current.Encrypt(Key, password);
		}
	}	

	public class AccountJsonConverter : JsonConverter<AccountAddress>
	{
		public override AccountAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return AccountAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, AccountAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
