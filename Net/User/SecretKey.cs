using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using NBitcoin.Secp256k1;

namespace Uccs.Net;

public class SecretKey
{
	private readonly ECPrivKey			ECKey;
	PublicKey						_Address;

	public byte[]						Secret { get; protected set; }
	public PublicKey				PuplicKey => _Address ??= new PublicKey(ECKey.CreateXOnlyPubKey().ToBytes(), Tag);
	public string						Tag { get; protected set; }
			
	public SecretKey(byte[] secret, string tag = null)
	{
		Secret = secret;
		ECKey = ECPrivKey.Create(secret);
		Tag = tag;
	}

	public static SecretKey Create(string tag = null)
	{
		var k = new byte[32];
		RandomNumberGenerator.Fill(k);

		return new SecretKey(k){Tag = tag};
	}

	public byte[] Sign(byte[] hash)
	{
		byte[] aux = new byte[32];
		RandomNumberGenerator.Fill(aux);

		SecpSchnorrSignature s;
		while(ECKey.TrySignBIP340(hash, null, out s) == false);

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
