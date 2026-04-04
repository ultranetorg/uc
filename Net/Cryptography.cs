using System.Security.Cryptography;
using Blake2Fast;
using Blake2Fast.Implementation;
using Org.BouncyCastle.Security;

namespace Uccs.Net;

public abstract class Cryptography
{
	public static readonly Cryptography				No = new NoCryptography();
	public static readonly Cryptography				Mcv = new McvCryptography();

	public const int								HashLength = 32;
	public const int								SignatureLength = 64;
	public const int								PrivateKeyLength = 32;
	public virtual byte[]							ZeroSignature => new byte[SignatureLength];
	public virtual byte[]							ZeroHash  => new byte[HashLength];

	public abstract CryptographyType				Type {get; }

	public abstract byte[]							Sign(AccountKey pk, byte[] hash);
	public abstract bool							Verify(AccountAddress address, byte[] hash, byte[] signature);

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

		///var c = Blake2b.CreateHashAlgorithm();
		///c.ComputeHash()
		
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

	public byte[] HashFile(Stream data)
	{
		return SHA256.HashData(data);
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

		Array.Copy(h, 0, s, 0, h.Length);
		Array.Copy(k.Address.Bytes, 0, s, 32, k.Address.Bytes.Length);

		return s;
	}

	public override bool Verify(AccountAddress address, byte[] hash, byte[] signature)
	{
		return Bytes.EqualityComparer.Equals(hash, signature[0..32]) && Bytes.EqualityComparer.Equals(address.Bytes, signature[32..64]);
	}
}

public class McvCryptography : Cryptography
{
	public override CryptographyType Type => CryptographyType.Mcv;

	public override byte[] Sign(AccountKey k, byte[] h)
	{
//		var sig = k.SignAndCalculateV(h);
//
//		var o = new byte[SignatureLength];
//
//		var r = sig.R.ToByteArrayUnsigned();
//		var s = sig.S.ToByteArrayUnsigned();
//
//		Array.Copy(r,	  0, o, 32 - r.Length,		r.Length);
//		Array.Copy(s,	  0, o, 32 + 32 - s.Length,	s.Length);
//		Array.Copy(sig.V, 0, o, 32 + 32,			1);

		return k.Sign(h);
	}

	public override bool Verify(AccountAddress address, byte[] hash, byte[] signature)
	{
// 		var r = new byte[32];
// 		var s = new byte[32];
// 		Array.Copy(signature, 0,	r, 0, r.Length);
// 		Array.Copy(signature, 32,	s, 0, s.Length);
// 	
//		var sig = new ECDSASignature(new Org.BouncyCastle.Math.BigInteger(1, r), 
//									 new Org.BouncyCastle.Math.BigInteger(1, s))
//									 {V = [signature[64]]};
//		
//		return AccountKey.RecoverFromSignature(sig, hash).Address;

		return AccountKey.Verify(address.Bytes, signature, hash);
	}
}

public class Blake2Stream : Stream
{
    Blake2bHashState			Hasher;
    bool						IsFinished;

    public override bool		CanRead => false;
    public override bool		CanSeek => false;
    public override bool		CanWrite => !IsFinished;
    public override long		Length => 0;
    public override long		Position 
    { 
        get => 0; 
        set => throw new NotSupportedException(); 
    }

    public override void Flush() { /* Nothing to flush */ }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public byte[] Hash
    {
		get
		{
			IsFinished = true;
		
			return Hasher.Finish();
		}
	}

    public Blake2Stream(int digestLength = Cryptography.HashLength)
    {
        Hasher = Blake2b.CreateIncrementalHasher(digestLength);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if(IsFinished) 
			throw new InvalidOperationException("Hash already finalized.");
        
        Hasher.Update(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if(IsFinished) 
			throw new InvalidOperationException("Hash already finalized.");
        
        Hasher.Update(buffer);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
		if(cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled(cancellationToken);

        try
        {
            if(IsFinished)
				throw new InvalidOperationException("Stream is closed/finished.");
            
            Hasher.Update(buffer.Span);
            return ValueTask.CompletedTask;
        }
        catch(Exception ex)
        {
            return ValueTask.FromException(ex);
        }
    }
}