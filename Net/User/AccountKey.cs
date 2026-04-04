using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using NBitcoin.Secp256k1;

namespace Uccs.Net;

public class AccountKey
{
	private readonly ECPrivKey			ECKey;
	AccountAddress						_Address;

	public byte[]						PrivateKey { get; protected set; }
	public AccountAddress				Address => _Address ??= new AccountAddress(ECKey.CreateXOnlyPubKey().ToBytes());

	static AccountKey()
	{
	}
		
	public AccountKey(byte[] vch)
	{
		PrivateKey = vch;
		ECKey = ECPrivKey.Create(vch);
	}

	internal AccountKey(ECPrivKey ecKey)
	{
		ECKey = ecKey;
	}

	public static AccountKey Create()
	{
		var k = new byte[32];
		RandomNumberGenerator.Fill(k);

		return new AccountKey(k);
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
