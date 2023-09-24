using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Net
{
	public class ZoneCreation
	{
		public class Father
		{
			string			PathWithoutExtention;
			AccountKey		_Key;
			byte[]			_NoCryptographyWallet;
			byte[]			_EthereumCryptographyWallet;

			public AccountKey Key
			{
				get
				{
					if(_Key == null)
						_Key = AccountKey.Load(Cryptography.No, System.IO.Path.Join(PathWithoutExtention + "." + Vault.NoCryptoWalletExtention), null);

					return _Key;
				}
			}

			public Father(string path)
			{
				PathWithoutExtention = path;
			}

			public byte[] GetWallet(Cryptography cryptography)
			{
				if(cryptography == Cryptography.No)
				{
					if(_NoCryptographyWallet == null)
						_NoCryptographyWallet = File.ReadAllBytes(System.IO.Path.Join(PathWithoutExtention + "." + Vault.NoCryptoWalletExtention));

					return _NoCryptographyWallet;
				}
				
				if(cryptography == Cryptography.Ethereum)
				{
					if(_EthereumCryptographyWallet == null)
						_EthereumCryptographyWallet = File.ReadAllBytes(System.IO.Path.Join(PathWithoutExtention + "." + Vault.EthereumWalletExtention));

					return _EthereumCryptographyWallet;
				}

				throw new ArgumentException();
			}
		}

		public Father			Gen;
		//public Father			UOFund;
		public Father[]			Fathers;
	}
}
