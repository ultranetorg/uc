using System.Buffers;
//using System.Security.Cryptography;
using System.Text;
using Blake2Fast;
using Blake2Fast.Implementation;
using NSec.Cryptography;

//using Konscious.Security.Cryptography;
using Org.BouncyCastle.Security;

namespace Uccs.Net;

public enum SigningFeatures
{
	None = 0,
	Deterministic
}

public abstract class Cryptography
{
	public static readonly Cryptography		No = new NoCryptography();
	public static readonly Cryptography		Mcv = new McvCryptography();
	public static readonly Cryptography		Iccp = new IccpCryptography();

	public const int						HashLength = 32;
	public const int						SignatureLength = 64;
	public const int						PrivateKeyLength = 32;
	public const int						PasswordSaltLength = 16;
	public virtual byte[]					ZeroSignature => new byte[SignatureLength];
	public virtual byte[]					ZeroHash  => new byte[HashLength];

	public abstract byte[]					Sign(SecretKey key, byte[] hash, SigningFeatures deterministic);
	public abstract bool					Verify(PublicKey key, byte[] hash, byte[] signature);
    public abstract byte[]					HashifyPassword(string password, byte[] salt);

	public abstract CryptographyType		Type {get; }
	public static readonly SecureRandom		Random = new ();

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
		
		//return Blake2Fast.Blake2b.ComputeHash(32, data);
		return NSec.Cryptography.Blake2b.Blake2b_256.Hash(data);
	}

	public static byte[] Hash(Span<byte> data)
	{
		return Blake2Fast.Blake2b.ComputeHash(32, data);
	}

	public static byte[] Hash(int length, byte[] data)
	{
		return Blake2Fast.Blake2b.ComputeHash(length, data);
	}

	public static byte[] Hash(byte[] a, byte[] b)
	{
		return Blake2Fast.Blake2b.ComputeHash(32, [..a, ..b]);
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
		return System.Security.Cryptography.SHA256.HashData(data);
	}

	public byte[] HashFile(Stream data)
	{
		return System.Security.Cryptography.SHA256.HashData(data);
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

	public override byte[] Sign(SecretKey k, byte[] h, SigningFeatures deterministic)
	{
		var s = new byte[SignatureLength];

		if(deterministic.HasFlag(SigningFeatures.Deterministic))
			Array.Copy(h, 0, s, 0, h.Length);
		else
		{	
			Array.Copy(h, 0, s, 0, h.Length/2);
			Random.NextBytes(s, h.Length/2, h.Length/2);
		}

		Array.Copy(k.Puplic.Bytes, 0, s, 32, k.Puplic.Bytes.Length);

		return s;
	}

	public override bool Verify(PublicKey key, byte[] hash, byte[] signature)
	{
		return Bytes.EqualityComparer.Equals(hash.AsSpan(0, 16), signature.AsSpan(0, 16)) && Bytes.EqualityComparer.Equals(key.Bytes, signature.AsSpan(32, 32));
	}

	public override byte[] HashifyPassword(string password, byte[] salt)
	{
		return Hash(Encoding.UTF8.GetBytes(password), salt);
	}
}

public class McvCryptography : Cryptography
{
	public override CryptographyType Type => CryptographyType.Mcv;
	static readonly Argon2Parameters Parameters =	new Argon2Parameters
													{
														MemorySize = 1024 * 1024,
														NumberOfPasses = 4,
														DegreeOfParallelism = 1
														
													};

	static readonly PasswordBasedKeyDerivationAlgorithm Kdf =  PasswordBasedKeyDerivationAlgorithm.Argon2id(Parameters);

	public override byte[] Sign(SecretKey k, byte[] h, SigningFeatures deterministic)
	{
		return k.Sign(h, deterministic);
	}

	public override bool Verify(PublicKey key, byte[] hash, byte[] signature)
	{
		return SecretKey.Verify(key.Bytes, signature, hash);
	}

	public override byte[] HashifyPassword(string password, byte[] salt)
	{
        int maxByteCount = Encoding.UTF8.GetMaxByteCount(password.Length);
        Span<byte> passwordBytes = stackalloc byte[maxByteCount];
        int actualBytes	= Encoding.UTF8.GetBytes(password, passwordBytes);
        Span<byte> cleanPasswordBytes = passwordBytes.Slice(0, actualBytes);

        byte[] derivedKey = new byte[HashLength];

        try
        {
            Kdf.DeriveBytes(cleanPasswordBytes, salt, derivedKey);
            return derivedKey;
        }
        finally
        {
            System.Security.Cryptography.CryptographicOperations.ZeroMemory(cleanPasswordBytes);
        }
	}
}

public class IccpCryptography : McvCryptography
{
	public override CryptographyType Type => CryptographyType.Iccp;
}

public class Blake2Stream : Stream
{
	private IncrementalHash		State;
	public byte[]				Hash => _Hash ??= IncrementalHash.Finalize(ref State);
	private byte[]				_Hash;

	public Blake2Stream()
	{
		IncrementalHash.Initialize(NSec.Cryptography.Blake2b.Blake2b_256, out State);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if(_Hash != null)
			throw new InvalidOperationException();

		if(count > 0)
		{
			IncrementalHash.Update(ref State, buffer.AsSpan(offset, count));
		}
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		if(_Hash != null)
			throw new InvalidOperationException();

		if(!buffer.IsEmpty)
		{
			IncrementalHash.Update(ref State, buffer);
		}
	}

	#region Stream Overrides

	public override bool CanRead => false;
	public override bool CanSeek => false;
	public override bool CanWrite => _Hash == null;
	public override long Length => throw new NotSupportedException();

	public override long Position
	{
		get => throw new NotSupportedException();
		set => throw new NotSupportedException();
	}

	public override void	Flush() { }
	public override int		Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	public override long	Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
	public override void	SetLength(long value) => throw new NotSupportedException();


	#endregion
}