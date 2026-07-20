using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Blake2Fast;
using Blake2Fast.Implementation;
using Konscious.Security.Cryptography;
using Org.BouncyCastle.Security;

namespace Uccs.Net;

public abstract class Cryptography
{
	public static readonly Cryptography				No = new NoCryptography();
	public static readonly Cryptography				Mcv = new McvCryptography();
	public static readonly Cryptography				Iccp = new IccpCryptography();

	public const int								HashLength = 32;
	public const int								SignatureLength = 64;
	public const int								PrivateKeyLength = 32;
	public virtual byte[]							ZeroSignature => new byte[SignatureLength];
	public virtual byte[]							ZeroHash  => new byte[HashLength];

	public abstract CryptographyType				Type {get; }

	public abstract byte[]							Sign(SecretKey pk, byte[] hash);
	public abstract bool							Verify(PublicKey address, byte[] hash, byte[] signature);
    public abstract byte[]							HashifyPassword(string password, byte[] salt);
													

	public static readonly SecureRandom				Random = new ();

	///[ThreadStatic]
	//public static DZen.Security.Cryptography.SHA3	SHA;


	protected Cryptography()
	{
	}

	public static Cryptography ByZone(Zone zone)
	{
		switch(zone)
		{
			case Zone.Simulation:
				return No;

			default:
				return Mcv;
		}
	}

	public static byte[] RandomBytes(int n)
	{
		var s = new byte[n];
		Random.NextBytes(s);
		return s;
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
	
	public static byte[] Hash(Action<BinaryWriter> write)
	{
		var s = new Blake2Stream();
		var w = new Writer(s);
		
		write(w);

		return s.Hash;
	}
	
	public static byte[] Hash(IEnumerable<IBinarySerializable> items)
	{
		var s = new Blake2Stream();
		var w = new Writer(s);
		
		foreach(var i in items)
			i.Write(w);

		return s.Hash;
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

	public override byte[] Sign(SecretKey k, byte[] h)
	{
		var s = new byte[SignatureLength];

		Array.Copy(h, 0, s, 0, h.Length);
		Array.Copy(k.PuplicKey.Bytes, 0, s, 32, k.PuplicKey.Bytes.Length);

		return s;
	}

	public override bool Verify(PublicKey address, byte[] hash, byte[] signature)
	{
		return Bytes.EqualityComparer.Equals(hash, signature[0..32]) && Bytes.EqualityComparer.Equals(address.Bytes, signature[32..64]);
	}

	public override byte[] HashifyPassword(string password, byte[] salt)
	{
		return Hash(Encoding.UTF8.GetBytes(password), salt);
	}
}

public class McvCryptography : Cryptography
{
	public override CryptographyType Type => CryptographyType.Mcv;

	public override byte[] Sign(SecretKey k, byte[] h)
	{
		return k.Sign(h);
	}

	public override bool Verify(PublicKey address, byte[] hash, byte[] signature)
	{
		return SecretKey.Verify(address.Bytes, signature, hash);
	}

	public override byte[] HashifyPassword(string password, byte[] salt)
	{
		const int MemorySizeInKb = 1024 * 1024;
		const int Iterations = 4;

		using var argon2 =	new Argon2id(Encoding.UTF8.GetBytes(password))
							{
								Salt = salt,
								DegreeOfParallelism = Environment.ProcessorCount,
								MemorySize = MemorySizeInKb,
								Iterations = Iterations
							};

		return argon2.GetBytes(HashLength);
	}
}

public class IccpCryptography : McvCryptography
{
	public override CryptographyType Type => CryptographyType.Iccp;
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