using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Nethereum.KeyStore;
using Nethereum.Signer;
using Nethereum.Util;
using Org.BouncyCastle.Security;

namespace Uccs.Net
{
	public abstract class Cryptography
	{
		public static readonly Cryptography	No = new NoCryptography();
		public static readonly Cryptography	Ethereum = new EthereumCryptography();

		public const int					SignatureSize = 65;
		public const int					HashSize = 32;
		public const int					PrivateKeyLength = 32;
		public virtual byte[]				ZeroSignature => new byte[SignatureSize];
		public virtual byte[]				ZeroHash  => new byte[HashSize];

		public abstract byte[]				Sign(AccountKey pk, byte[] hash);
		public abstract AccountAddress		AccountFrom(byte[] signature, byte[] hash);
		public abstract byte[]				Encrypt(EthECKey key, string password);
		public abstract byte[]				Decrypt(byte[] input, string password);

		public static readonly SecureRandom	Random = new SecureRandom();

		protected Cryptography()
		{
		}

		public byte[] Hash(byte[] data)
		{
			return Sha3Keccack.Current.CalculateHash(data);
		}

		public byte[] HashFile(byte[] data)
		{
			return SHA256.HashData(data);
		}

// 		public byte[] Hash(byte[] data)
// 		{
// 			return Sha3Keccack.Current.CalculateHash(data);
// 		}

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

		public override byte[] Encrypt(EthECKey key, string password)
		{
			//if(string.IsNullOrWhiteSpace(password))
			//	throw new RequirementException("Non-empty password required");
			//
			//var pkey = Hash(Encoding.UTF8.GetBytes(password));
			//
			//var e = new AesEngine();
			//
			//var b = new PaddedBufferedBlockCipher(e);
			//b.Init(true, new KeyParameter(pkey));
			//
			//return b.DoFinal(key.GetPrivateKeyAsBytes());

			return key.GetPrivateKeyAsBytes();
		}

		public override byte[] Decrypt(byte[] input, string password)
		{
			////if(string.IsNullOrWhiteSpace(password))
			////	throw new UserException("Non-empty password required");
			//
			//var key = Hash(Encoding.UTF8.GetBytes(password));
			//
			//var e = new AesEngine();
			//
			//var b = new PaddedBufferedBlockCipher(e);
			//b.Init(false, new KeyParameter(key));
			//
			//return b.DoFinal(input);

			return input;
		}
	}

	public class EthereumCryptography : Cryptography
	{
		static KeyStoreService	service;

		static EthereumCryptography()
		{
			EthECKey.SignRecoverable = true;
			service = new KeyStoreService();
		}

		public override byte[] Sign(AccountKey k, byte[] h)
		{
			var sig = k.Key.SignAndCalculateV(h);
	
			var s = new byte[SignatureSize];
	
			Array.Copy(sig.R, 0, s, 32 - sig.R.Length,		sig.R.Length);
			Array.Copy(sig.S, 0, s, 32 + 32 - sig.S.Length,	sig.S.Length);
			Array.Copy(sig.V, 0, s, 32 + 32,				1);
	
			//if(new Account(k) != AccountFrom(s, h))
			//	throw new IntegrityException("Member-Signature inconsistency");						
	
			return s;
		}

		public override AccountAddress AccountFrom(byte[] signature, byte[] hash)
		{
			var r = new byte[32];
			var s = new byte[32];
			Array.Copy(signature, 0,	r, 0, r.Length);
			Array.Copy(signature, 32,	s, 0, s.Length);
	
			var sig = EthECDSASignatureFactory.FromComponents(r, s, signature[64]);
	
			return new AccountAddress(EthECKey.RecoverFromSignature(sig, hash));
		}

		public override byte[] Encrypt(EthECKey key, string password)
		{
			return Encoding.UTF8.GetBytes(service.EncryptAndGenerateDefaultKeyStoreAsJson(password, key.GetPrivateKeyAsBytes(), key.GetPublicAddress()));
		}

		public override byte[] Decrypt(byte[] input, string password)
		{
			return service.DecryptKeyStoreFromJson(password, Encoding.UTF8.GetString(input));
		}
	}

}
