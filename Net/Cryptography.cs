using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Blake2Fast;
using Org.BouncyCastle.Security;

namespace Uccs.Net;

public abstract class Cryptography
{
	public static readonly Cryptography				No = new NoCryptography();
	public static readonly Cryptography				Mcv = new McvCryptography();

	public const int								SignatureLength = 65;
	public const int								HashSize = 32;
	public const int								PrivateKeyLength = 32;
	public virtual byte[]							ZeroSignature => new byte[SignatureLength];
	public virtual byte[]							ZeroHash  => new byte[HashSize];

	public abstract CryptographyType				Type {get; }

	public abstract byte[]							Sign(AccountKey pk, byte[] hash);
	public abstract AccountAddress					AccountFrom(byte[] signature, byte[] hash);

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

	public static byte[] Hash(Span<byte> data)
	{
		return Blake2b.ComputeHash(32, data);
	}

	public static byte[] Hash(int length, byte[] data)
	{
		return Blake2b.ComputeHash(length, data);
	}

	public static byte[] Hash(byte[] a, byte[] b)
	{
		return Blake2b.ComputeHash(32, [..a, ..b]);
	}

	public byte[] HashFile(byte[] data)
	{
		return SHA256.HashData(data);
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
	public override CryptographyType Type => CryptographyType.No;

	public override byte[] Sign(AccountKey k, byte[] h)
	{
		var s = new byte[SignatureLength];

		Array.Copy(k.Address.Bytes, 0, s, 0, k.Address.Bytes.Length);
		Array.Copy(h, 0, s, 32,	h.Length);

		return s;
	}

	public override AccountAddress AccountFrom(byte[] signature, byte[] hash)
	{
		return new AccountAddress(signature.Take(AccountAddress.Length).ToArray());
	}
}

public class McvCryptography : Cryptography
{
	public override CryptographyType Type => CryptographyType.Mcv;

	public override byte[] Sign(AccountKey k, byte[] h)
	{
		var sig = k.SignAndCalculateV(h);

		var o = new byte[SignatureLength];

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
		
		return AccountKey.RecoverFromSignature(sig, hash).Address;
	}
}
