using NBitcoin.Secp256k1;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;


namespace Uccs.Net;

/// <summary>
///     ECKey based on the implementation of bitcoinj, NBitcoin
/// </summary>
public class ECKey
{
	public static readonly BigInteger HALF_CURVE_ORDER;
	public static readonly BigInteger CURVE_ORDER;
	public static readonly ECDomainParameters CURVE;
	internal static readonly X9ECParameters _secp256k1;
	private readonly ECKeyParameters _Key;
	private ECPublicKeyParameters _ecPublicKeyParameters;
	private byte[] _publicKey;
	private byte[] _publicKeyCompressed;


	private ECDomainParameters _DomainParameter;

	private static readonly BigInteger PRIME;

	static ECKey()
	{
		//using Bouncy
		_secp256k1 = SecNamedCurves.GetByName("secp256k1");
		CURVE = new ECDomainParameters(_secp256k1.Curve, _secp256k1.G, _secp256k1.N, _secp256k1.H);
		HALF_CURVE_ORDER = _secp256k1.N.ShiftRight(1);
		CURVE_ORDER = _secp256k1.N;
		PRIME = new BigInteger(1,
		   Org.BouncyCastle.Utilities.Encoders.Hex.Decode(
			   "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F"));
	}

	public ECKey(byte[] vch, bool isPrivate)
	{
		if(isPrivate)
		{
			_Key = new ECPrivateKeyParameters(new BigInteger(1, vch), DomainParameter);
		}
		else
		{
			var q = Secp256k1.Curve.DecodePoint(vch);
			_Key = new ECPublicKeyParameters("EC", q, DomainParameter);
		}
	}

	public ECPrivateKeyParameters PrivateKey => _Key as ECPrivateKeyParameters;


	public static X9ECParameters Secp256k1 => _secp256k1;

	public ECDomainParameters DomainParameter
	{
		get
		{
			if(_DomainParameter == null)
				_DomainParameter = new ECDomainParameters(Secp256k1.Curve, Secp256k1.G, Secp256k1.N, Secp256k1.H);
			return _DomainParameter;
		}
	}


	public byte[] GetPubKey(bool isCompressed)
	{
		if(_publicKey != null && !isCompressed) return _publicKey;
		if(_publicKeyCompressed != null && isCompressed) return _publicKeyCompressed;

		var q = GetPublicKeyParameters().Q;
		//Pub key (q) is composed into X and Y, the compressed form only include X, which can derive Y along with 02 or 03 prepent depending on whether Y in even or odd.
		q = q.Normalize();

		if(isCompressed)
		{
			_publicKeyCompressed =
			Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(true);
			return _publicKeyCompressed;

		}
		else
		{
			var _publicKey =
			Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(false);
			return _publicKey;
		}
	}

	public ECPublicKeyParameters GetPublicKeyParameters()
	{
		if(_ecPublicKeyParameters == null)
		{
			if(_Key is ECPublicKeyParameters)
				_ecPublicKeyParameters = (ECPublicKeyParameters)_Key;
			else
			{
				var q = Secp256k1.G.Multiply(PrivateKey.D);
				_ecPublicKeyParameters = new ECPublicKeyParameters("EC", q, DomainParameter);

			}
		}
		return _ecPublicKeyParameters;
	}




	public static ECKey RecoverFromSignature(int recId, ECDSASignature sig, byte[] message, bool compressed)
	{

		if(recId < 0)
			throw new ArgumentException("recId should be positive");
		if(sig.R.SignValue < 0)
			throw new ArgumentException("r should be positive");
		if(sig.S.SignValue < 0)
			throw new ArgumentException("s should be positive");
		if(message == null)
			throw new ArgumentNullException("message");


		SecpECDSASignature.TryCreateFromDer(sig.ToDER(), out var signature);
		var recoverable = new SecpRecoverableECDSASignature(signature, recId);
		ECPubKey.TryRecover(Context.Instance, recoverable, message, out var pubKey);
		return new ECKey(pubKey.ToBytes(compressed), false);

	}


	public virtual ECDSASignature Sign(byte[] hash)
	{

		AssertPrivateKey();
		var signer = new DeterministicECDSA();
		signer.setPrivateKey(PrivateKey);
		var sig = ECDSASignature.FromDER(signer.signHash(hash));
		return sig.MakeCanonical();
	}

	public bool Verify(byte[] hash, ECDSASignature sig)
	{
		var signer = new ECDsaSigner();
		signer.Init(false, GetPublicKeyParameters());
		return signer.VerifySignature(hash, sig.R, sig.S);
	}

	private void AssertPrivateKey()
	{
		if(PrivateKey == null)
			throw new InvalidOperationException("This key should be a private key for such operation");
	}

}

public class ECDSASignature
{
	private const string InvalidDERSignature = "Invalid DER signature";

	public BigInteger R { get; }
	public BigInteger S { get; }
	public byte[] V { get; set; }
	public bool IsLowS => S.CompareTo(ECKey.HALF_CURVE_ORDER) <= 0;

	public ECDSASignature(BigInteger r, BigInteger s)
	{
		R = r;
		S = s;
	}

	public ECDSASignature(BigInteger[] rs)
	{
		R = rs[0];
		S = rs[1];
	}

	public ECDSASignature(byte[] derSig)
	{
		try
		{
			var decoder = new Asn1InputStream(derSig);
			var seq = decoder.ReadObject() as DerSequence;
			if(seq == null || seq.Count != 2)
				throw new FormatException(InvalidDERSignature);
			R = ((DerInteger)seq[0]).Value;
			S = ((DerInteger)seq[1]).Value;
		}
		catch(Exception ex)
		{
			throw new FormatException(InvalidDERSignature, ex);
		}
	}

	public static ECDSASignature FromDER(byte[] sig)
	{
		return new ECDSASignature(sig);
	}

	public static bool IsValidDER(byte[] bytes)
	{
		try
		{
			FromDER(bytes);
			return true;
		}
		catch(FormatException)
		{
			return false;
		}
		catch(Exception)
		{
			return false;
		}
	}

	/// <summary>
	///     Enforce LowS on the signature
	/// </summary>
	public ECDSASignature MakeCanonical()
	{
		if(!IsLowS)
			return new ECDSASignature(R, ECKey.CURVE_ORDER.Subtract(S));
		return this;
	}

	/**
	* What we get back from the signer are the two components of a signature, r and s. To get a flat byte stream
	* of the type used by Bitcoin we have to encode them using DER encoding, which is just a way to pack the two
	* components into a structure.
	*/

	public byte[] ToDER()
	{
		// Usually 70-72 bytes.
		var bos = new MemoryStream(72);
		var seq = new DerSequenceGenerator(bos);
		seq.AddObject(new DerInteger(R));
		seq.AddObject(new DerInteger(S));
		seq.Close();
		return bos.ToArray();
	}
}

internal class DeterministicECDSA : ECDsaSigner
{
	private readonly IDigest _digest;
	private byte[] _buffer = new byte[0];

	public DeterministicECDSA()
		: base(new HMacDsaKCalculator(new Sha256Digest()))

	{
		_digest = new Sha256Digest();
	}

	public DeterministicECDSA(Func<IDigest> digest)
		: base(new HMacDsaKCalculator(digest()))
	{
		_digest = digest();
	}


	public void setPrivateKey(ECPrivateKeyParameters ecKey)
	{
		Init(true, ecKey);
	}

	public byte[] sign()
	{
		var hash = new byte[_digest.GetDigestSize()];
		_digest.BlockUpdate(_buffer, 0, _buffer.Length);
		_digest.DoFinal(hash, 0);
		_digest.Reset();
		return signHash(hash);
	}

	public byte[] signHash(byte[] hash)
	{
		return new ECDSASignature(GenerateSignature(hash)).ToDER();
	}

	public void update(byte[] buf)
	{
		_buffer = _buffer.Concat(buf).ToArray();
	}
}