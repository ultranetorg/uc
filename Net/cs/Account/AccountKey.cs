using System;
using System.IO;
using NBitcoin.Secp256k1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Uccs.Net
{
	public class AccountKey : AccountAddress
	{
// 		[JsonIgnore]
// 
// 		public AccountKey(AccountKey k)
// 		{
// 			var initaddr = Sha3Keccack.Current.CalculateHash(k.GetPubKeyNoPrefix());
// 			Bytes = new byte[initaddr.Length - 12];
// 			Array.Copy(initaddr, 12, Bytes, 0, initaddr.Length - 12);
// 
// 			if(Bytes.Length != Length)
// 				throw new IntegrityException("Bytes.Length != Length");
// 		}
// 
// 		public AccountKey(byte[] privatekay)
// 		{
// 			Key = new AccountKey(privatekay, true);
// 
// 			var initaddr = Sha3Keccack.Current.CalculateHash(Key.GetPubKeyNoPrefix());
// 			Bytes = new byte[initaddr.Length - 12];
// 			Array.Copy(initaddr, 12, Bytes, 0, initaddr.Length - 12);
// 		}
// 	
        public override byte[]	Bytes { get => GetPublicAddressAsBytes(); protected set => throw new NotSupportedException(); }

		public new static AccountKey Parse(string privatekay)
		{
			return new AccountKey(privatekay);
		}

		public static AccountKey Load(Cryptography cryptography, string path, string password)
		{
			return cryptography.Decrypt(File.ReadAllBytes(path), password);
		}

		public static AccountKey Load(Cryptography cryptography, byte[] wallet, string password)
		{
			return cryptography.Decrypt(wallet, password);
		}

		public void Save(Cryptography cryptography, string path, string password)
		{
			File.WriteAllBytes(path, cryptography.Encrypt(this, password));
		}

		public byte[] Save(Cryptography cryptography, string password)
		{
			return cryptography.Encrypt(this, password);
		}

        private static readonly SecureRandom SecureRandom = new SecureRandom();

#if NETCOREAPP3_1 || NET5_0_OR_GREATER
        /// <summary>
        /// Enables / Disables whilst signing creating a recoverable id, as opposed to afterward. When enabled this uses NBitcoin.Secp256k1 as opposed to BouncyCastle to create the signature.
        /// </summary>
        public static bool SignRecoverable { get; set; } = false;
#endif
        public static byte DEFAULT_PREFIX = 0x04;
        private readonly ECKey _ecKey;
        private byte[] _publicKey;
        private byte[] _publicKeyCompressed;
        private byte[] _publicKeyNoPrefix;
        private byte[] _publicKeyNoPrefixCompressed;
        private string _Address;
        private byte[] _privateKey;
        private string _privateKeyHex;


        public AccountKey(string privateKey)
        {
            _ecKey = new ECKey(privateKey.Substring(Prefix.Length).FromHex(), true);
        }


        public AccountKey(byte[] vch)
        {
            _ecKey = new ECKey(vch, true);
        }

        internal AccountKey(ECKey ecKey)
        {
            _ecKey = ecKey;
        }

        public static AccountKey Create(byte[] seed = null)
        {
            var secureRandom = SecureRandom;
            if (seed != null)
            {
                secureRandom = new SecureRandom();
                secureRandom.SetSeed(seed);
            }

            var gen = new ECKeyPairGenerator("EC");
            var keyGenParam = new KeyGenerationParameters(secureRandom, 256);
            gen.Init(keyGenParam);
            var keyPair = gen.GenerateKeyPair();
            var privateBytes = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();
            if (privateBytes.Length != 32)
                return Create();
            return new AccountKey(privateBytes);
        }

        public static AccountKey Create()
        {
            var gen = new ECKeyPairGenerator("EC");
            var keyGenParam = new KeyGenerationParameters(SecureRandom, 256);
            gen.Init(keyGenParam);
            var keyPair = gen.GenerateKeyPair();
            var privateBytes = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();
            if (privateBytes.Length != 32)
                return Create();
            return new AccountKey(privateBytes);
        }

        public byte[] GetPrivateKeyAsBytes()
        {
            if (_privateKey == null)
            {
                _privateKey = _ecKey.PrivateKey.D.ToByteArrayUnsigned();
            }
            return _privateKey;
        }

        public string GetPrivateKey()
        {
            if (_privateKeyHex == null)
            {
                _privateKeyHex = Prefix + GetPrivateKeyAsBytes().ToHex();
            }
            return _privateKeyHex;
        }

        public byte[] GetPubKey(bool compresseed = false)
        {
            if (!compresseed)
            {
                if (_publicKey == null)
                {
                    _publicKey = _ecKey.GetPubKey(false);
                }
                return _publicKey;
            }
            else
            {
                if (_publicKeyCompressed == null)
                {
                    _publicKeyCompressed = _ecKey.GetPubKey(true);
                }
                return _publicKeyCompressed;

            }
        }

        public byte[] GetPubKeyNoPrefix(bool compressed = false)
        {
            if (!compressed)
            {
                if (_publicKeyNoPrefix == null)
                {
                    var pubKey = _ecKey.GetPubKey(false);
                    var arr = new byte[pubKey.Length - 1];
                    //remove the prefix
                    Array.Copy(pubKey, 1, arr, 0, arr.Length);
                    _publicKeyNoPrefix = arr;
                }
                return _publicKeyNoPrefix;
            }
            else
            {
                if (_publicKeyNoPrefixCompressed == null)
                {
                    var pubKey = _ecKey.GetPubKey(true);
                    var arr = new byte[pubKey.Length - 1];
                    //remove the prefix
                    Array.Copy(pubKey, 1, arr, 0, arr.Length);
                    _publicKeyNoPrefixCompressed = arr;
                }
                return _publicKeyNoPrefixCompressed;

            }
        }

        public string GetPublicAddress()
        {
            if (_Address == null)
            {
                //var initaddr = new Sha3Keccack().CalculateHash(GetPubKeyNoPrefix());
                var initaddr = Cryptography.Hash(GetPubKeyNoPrefix());
                var addr = new byte[initaddr.Length - 12];
                Array.Copy(initaddr, 12, addr, 0, initaddr.Length - 12);
                _Address = Prefix + addr.ToHex();
            }
            return _Address;
        }

        public byte[] GetPublicAddressAsBytes()
        {
            if (_Address == null)
            {
                var initaddr = Cryptography.Hash(GetPubKeyNoPrefix());
                var addr = new byte[initaddr.Length - 12];
                Array.Copy(initaddr, 12, addr, 0, initaddr.Length - 12);
                return addr;
            }
            return _Address.FromHex();
        }


        public static AccountKey RecoverFromSignature(ECDSASignature signature, byte[] hash)
        {
            return new AccountKey(ECKey.RecoverFromSignature(signature.V[0], signature, hash, false));
        }

        public ECDSASignature SignAndCalculateV(byte[] hash)
        {
            var privKey = Context.Instance.CreateECPrivKey(GetPrivateKeyAsBytes());
            privKey.TrySignRecoverable(hash, out var recSignature);
            recSignature.Deconstruct(out var r, out var s, out var recId);
            return new ECDSASignature(new BigInteger(1, r.ToBytes()), new BigInteger(1, s.ToBytes())){ V = [(byte)recId]};
        }
    }
}