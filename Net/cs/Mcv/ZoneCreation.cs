using System.IO;

namespace Uccs.Net
{
	public class ZoneCreation
	{
		public class Father
		{
			string			PathWithoutExtention;
			AccountKey		_Key;
			byte[]			_PrivateKey;

			public string	EthereumWalletPath => System.IO.Path.Join(PathWithoutExtention + "." + Vault.EncryptedWalletExtention);

			public AccountKey Key
			{
				get
				{
					if(_Key == null)
						_Key = new AccountKey(PrivateKey);

					return _Key;
				}
			}

			public byte[] PrivateKey
			{
				get
				{
					if(_PrivateKey == null)
						_PrivateKey = File.ReadAllBytes(System.IO.Path.Join(PathWithoutExtention + "." + Vault.PrivakeKeyWalletExtention));

					return _PrivateKey;
				}
			}

			public Father(string path)
			{
				PathWithoutExtention = path;
			}
		}

		public Father			Gen;
		//public Father			UOFund;
		public Father[]			Fathers;
	}
}
