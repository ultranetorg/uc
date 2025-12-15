using System.Text.Json;
using System.Text.Json.Serialization;
using NBitcoin.Secp256k1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Uccs.Net;

public class AccountKey
{
	private readonly ECKey		ECKey;
	private byte[]				_PublicKeyNoPrefix;
	private byte[]				_PublicKeyNoPrefixCompressed;
	private byte[]				_PrivateKey;
	AccountAddress				_Address;

	static ECKeyPairGenerator	Generator;

	static AccountKey()
	{
		Generator = new ECKeyPairGenerator("EC");
		Generator.Init(new KeyGenerationParameters(Cryptography.Random, 256));
	}

	public AccountAddress Address 
	{
		get
		{
			if(_Address == null)
			{
				var initaddr = Cryptography.Hash(GetPubKeyNoPrefix());
				var a  = new byte[initaddr.Length - 12];
				Array.Copy(initaddr, 12, a, 0, initaddr.Length - 12);

				_Address = new AccountAddress(a);
			}

			return _Address;
		}
	}

	public byte[] PrivateKey
	{
		get
		{
			if(_PrivateKey == null)
			{
				_PrivateKey = ECKey.PrivateKey.D.ToByteArrayUnsigned();
			}
			return _PrivateKey;
		}
	}

	public AccountKey(byte[] vch)
	{
		ECKey = new ECKey(vch, true);
	}

	internal AccountKey(ECKey ecKey)
	{
		ECKey = ecKey;
	}

	public static AccountKey Create(byte[] seed = null)
	{
		var secureRandom = Cryptography.Random;

		if(seed != null)
		{
			secureRandom = new SecureRandom();
			secureRandom.SetSeed(seed);
		}

		var keyPair = Generator.GenerateKeyPair();
		var privateBytes = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();

		if (privateBytes.Length != 32)
			return Create();

		return new AccountKey(privateBytes);
	}

	public static AccountKey Create()
	{
		var keyPair = Generator.GenerateKeyPair();
		var privateBytes = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();

		if (privateBytes.Length != 32)
			return Create();

		return new AccountKey(privateBytes);
	}

	public byte[] GetPubKeyNoPrefix(bool compressed = false)
	{
		if(!compressed)
		{
			if(_PublicKeyNoPrefix == null)
			{
				var pubKey = ECKey.GetPubKey(false);
				var arr = new byte[pubKey.Length - 1];
				//remove the prefix
				Array.Copy(pubKey, 1, arr, 0, arr.Length);
				_PublicKeyNoPrefix = arr;
			}
			return _PublicKeyNoPrefix;
		}
		else
		{
			if (_PublicKeyNoPrefixCompressed == null)
			{
				var pubKey = ECKey.GetPubKey(true);
				var arr = new byte[pubKey.Length - 1];
				//remove the prefix
				Array.Copy(pubKey, 1, arr, 0, arr.Length);
				_PublicKeyNoPrefixCompressed = arr;
			}
			return _PublicKeyNoPrefixCompressed;
		}
	}

	public static AccountKey RecoverFromSignature(ECDSASignature signature, byte[] hash)
	{
		return new AccountKey(ECKey.RecoverFromSignature(signature.V[0], signature, hash, false));
	}

	public ECDSASignature SignAndCalculateV(byte[] hash)
	{
		var privKey = Context.Instance.CreateECPrivKey(PrivateKey);
		privKey.TrySignRecoverable(hash, out var recSignature);
		recSignature.Deconstruct(out var r, out var s, out var recId);

		return new ECDSASignature(new BigInteger(1, r.ToBytes()), new BigInteger(1, s.ToBytes())){ V = [(byte)recId]};
	}
}


public class AccountKeyJsonConverter : JsonConverter<AccountKey>
{
	public override AccountKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new AccountKey(reader.GetString().FromHex());
	}

	public override void Write(Utf8JsonWriter writer, AccountKey value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.PrivateKey.ToHex());
	}
}
