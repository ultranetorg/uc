using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Blake2Fast;
using Org.BouncyCastle.Security;

namespace Uccs.Net;

public abstract class Cryptography
{
	public static readonly Cryptography				No = new NoCryptography();
	public static readonly Cryptography				Normal = new NormalCryptography();

	public const int								SignatureSize = 65;
	public const int								HashSize = 32;
	public const int								PrivateKeyLength = 32;
	public virtual byte[]							ZeroSignature => new byte[SignatureSize];
	public virtual byte[]							ZeroHash  => new byte[HashSize];

	public abstract byte[]							Sign(AccountKey pk, byte[] hash);
	public abstract AccountAddress					AccountFrom(byte[] signature, byte[] hash);
	public abstract byte[]							Encrypt(byte[] input, string password);
	public abstract byte[]							Decrypt(byte[] input, string password);

	public static readonly SecureRandom				Random = new SecureRandom();

	///[ThreadStatic]
	//public static DZen.Security.Cryptography.SHA3	SHA;

	protected Cryptography()
	{
	}

	public static byte[] Hash(byte[] data)
	{
		//if(SHA == null)
		//{
		//	SHA = new DZen.Security.Cryptography.SHA3256Managed();
		//	SHA.UseKeccakPadding = true;
		//}
		//
		//return SHA.ComputeHash(data);
		//return Sha3Keccack.Current.CalculateHash(data);
		
		//return SHA256.HashData(data);
		return Blake2b.ComputeHash(32, data);
	}

	public static byte[] Hash(byte[] iv, byte[] data)
	{
		//if(SHA == null)
		//{
		//	SHA = new DZen.Security.Cryptography.SHA3256Managed();
		//	SHA.UseKeccakPadding = true;
		//}
		//
		//return SHA.ComputeHash(data);
		//return Sha3Keccack.Current.CalculateHash(data);
		
		//return SHA256.HashData(data);
		return Blake2b.ComputeHash(32, iv, data);
	}

	public byte[] HashFile(byte[] data)
	{
		return System.Security.Cryptography.SHA256.HashData(data);
	}

	public virtual bool Valid(byte[] signature, byte[] hash, AccountAddress a)
	{
		return AccountFrom(signature, hash) == a;
	}

	public byte[] ToBytes(int n)
	{
		var b = BitConverter.GetBytes(n);
		return BitConverter.IsLittleEndian ? b : b.Reverse().ToArray();
	}
}

public class NoCryptography : Cryptography
{
	public override byte[] Sign(AccountKey k, byte[] h)
	{
		var s = new byte[SignatureSize];

		Array.Copy(k.Bytes, 0, s, 0, k.Bytes.Length);
		Array.Copy(h, 0, s, 32,	h.Length);

		return s;
	}

	public override AccountAddress AccountFrom(byte[] signature, byte[] hash)
	{
		return new AccountAddress(signature.Take(AccountAddress.Length).ToArray());
	}

	public override byte[] Encrypt(byte[] key, string password)
	{
		return key;
	}

	public override byte[] Decrypt(byte[] input, string password)
	{
		return input;
	}

}

public class NormalCryptography : Cryptography
{
	public override byte[] Sign(AccountKey k, byte[] h)
	{
		var sig = k.SignAndCalculateV(h);

		var o = new byte[SignatureSize];

		var r = sig.R.ToByteArrayUnsigned();
		var s = sig.S.ToByteArrayUnsigned();

		Array.Copy(r,	  0, o, 32 - r.Length,		r.Length);
		Array.Copy(s,	  0, o, 32 + 32 - s.Length,	s.Length);
		Array.Copy(sig.V, 0, o, 32 + 32,			1);

		return o;
	}

	public override AccountAddress AccountFrom(byte[] signature, byte[] hash)
	{
 		var r = new byte[32];
 		var s = new byte[32];
 		Array.Copy(signature, 0,	r, 0, r.Length);
 		Array.Copy(signature, 32,	s, 0, s.Length);
 	
		var sig = new ECDSASignature(new Org.BouncyCastle.Math.BigInteger(1, r), 
									 new Org.BouncyCastle.Math.BigInteger(1, s))
									 {V = [signature[64]]};
		
		return new AccountAddress(AccountKey.RecoverFromSignature(sig, hash));
	}

	public override byte[] Encrypt(byte[] data, string password)
	{
        byte[] iv = RandomNumberGenerator.GetBytes(16);

		using(Aes aesAlg = Aes.Create())
		{
			aesAlg.Key = Hash(Encoding.UTF8.GetBytes(password));
			aesAlg.IV = iv;
			ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
			
			byte[] en;
			
			using(var msEncrypt = new MemoryStream())
			{
				using(var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
				{
					csEncrypt.Write(data, 0, data.Length);
				}
				en = msEncrypt.ToArray();
			}

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			//w.WriteBytes(key.GetPublicAddressAsBytes());
			w.WriteBytes(iv);
			w.WriteBytes(en);

			return s.ToArray();
		}
	}

	public override byte[] Decrypt(byte[] data, string password)
	{
		var s = new MemoryStream(data);
		var r = new BinaryReader(s);
			
		//var pub = r.ReadBytes();
		var iv = r.ReadBytes();
		var en = r.ReadBytes();

		using(Aes aesAlg = Aes.Create())
		{
			aesAlg.Key = Hash(Encoding.UTF8.GetBytes(password));
			aesAlg.IV = iv;

			ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
			byte[] de;

			using(var msDecrypt = new MemoryStream(en))
			{
				using(var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
				{
					using(var msPlain = new MemoryStream())
					{
						csDecrypt.CopyTo(msPlain);
						de = msPlain.ToArray();
					}
				}
			}

			return de;
		}
	}
}
