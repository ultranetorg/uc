using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using NBitcoin.Secp256k1;

namespace Uccs.Net;

public class SecretKey
{
	readonly ECPrivKey				Private;
	PublicKey						_Public;

	public byte[]					Secret { get; protected set; }
	public PublicKey				PuplicKey => _Public ??= new PublicKey(Private.CreateXOnlyPubKey().ToBytes(), Tag);
	public string					Tag { get; protected set; }
			
	readonly BIP340NonceFunction	Deterministic = new BIP340NonceFunction(new byte[32]);

	public SecretKey(byte[] secret, string tag = null)
	{
		Secret = secret;
		Private = ECPrivKey.Create(secret);
		Tag = tag;
	}

	public static SecretKey Create(string tag = null)
	{
		var k = new byte[32];
		RandomNumberGenerator.Fill(k);

		return new SecretKey(k){Tag = tag};
	}

	public byte[] Sign(byte[] hash, SigningFeatures features)
	{
		SecpSchnorrSignature s;

		while(!Private.TrySignBIP340(hash, features.HasFlag(SigningFeatures.Deterministic) ? Deterministic  : null, out s))
			;

		return s.ToBytes();
	}

	public static bool Verify(byte[] publickey,  byte[] signature, byte[] hash)
	{
		if(!SecpSchnorrSignature.TryCreate(signature, out var s))
			return false;

		return ECXOnlyPubKey.Create(publickey).SigVerifyBIP340(s, hash);
	}

	public override string ToString()
	{
		return Tag != null ? $"{Tag}/{Secret.ToHex()}" : Secret.ToHex();
	}

	public static SecretKey Parse(string text)
	{
		var i = text.IndexOf('/');

		if(i == 0)
		{
			return new SecretKey(text.FromHex());
		} 
		else
		{
			return new SecretKey(text.AsSpan(i + 1).FromHex(), text.Substring(0, i));
		}
	}
}

public class SecretKeyJsonConverter : JsonConverter<SecretKey>
{
	public override SecretKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new SecretKey(reader.GetString().FromHex());
	}

	public override void Write(Utf8JsonWriter writer, SecretKey value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Secret.ToHex());
	}
}
