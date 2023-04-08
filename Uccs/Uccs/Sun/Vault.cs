using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Model;
using Nethereum.Signer;
using Org.BouncyCastle.Crypto;

namespace Uccs.Net
{
	public class Vault
	{
		public const string							EthereumWalletExtention = "uwe";
		public const string							NoCryptoWalletExtention = "uwnc";
		public static string						WalletExt(Cryptography c) => c is EthereumCryptography ? EthereumWalletExtention : NoCryptoWalletExtention;

		Zone										Zone;
		Settings									Settings;
		Log											Log;
		public Dictionary<AccountAddress, byte[]>	Wallets = new();
		public List<AccountAddress>					Accounts = new();
		public Dictionary<AccountAddress, int>		OperationIds = new();

		public event Action							AccountsChanged;						

		public readonly static string[]				PasswordWarning = {	"There is no way to recover Ultranet Account passwords. Back it up in some reliable location.",
																		"Make it long. This is the most critical factor. Choose nothing shorter than 15 characters, more if possible.",
																		"Use a mix of characters. The more you mix up letters (upper-case and lower-case), numbers, and symbols, the more potent your password is, and the harder it is for a brute force attack to crack it.",
																		"Avoid common substitutions. Password crackers are hip to the usual substitutions. Whether you use DOORBELL or D00R8377, the brute force attacker will crack it with equal ease.",
																		"Don’t use memorable keyboard paths. Much like the advice above not to use sequential letters and numbers, do not use sequential keyboard paths either (like qwerty)."};

		public Vault(Zone zone, Settings settings, Log log)
		{
			Zone = zone;
			Settings = settings;
			Log		 = log;

			Directory.CreateDirectory(Settings.Profile);

			if(Directory.Exists(Settings.Profile))
			{
				foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + WalletExt(Zone.Cryptography)))
				{
					Wallets[AccountAddress.Parse(Path.GetFileNameWithoutExtension(i))] = File.ReadAllBytes(i);
					Accounts.Add(AccountAddress.Parse(Path.GetFileNameWithoutExtension(i)));
				}
			}
		}

		public void AddWallet(AccountAddress account, byte[] wallet)
		{
			Accounts.Add(account);
			Wallets[account] = wallet;
			
			Log?.Report(this, "Wallet added", account.ToString());
		}

		public AccountKey Unlock(AccountAddress a, string password)
		{
			var p = AccountKey.Load(Zone.Cryptography, Wallets[a], password);

			var i = Accounts.IndexOf(a);
			Accounts.Remove(a);
			Accounts.Insert(i, p);

			Log?.Report(this, "Wallet unlocked", a.ToString());

			return p;
		}

		public AccountKey GetKey(AccountAddress a)
		{
			return Accounts.Find(i => i == a) as AccountKey;
		}

		public string AddWallet(AccountKey a, string password)
		{
			AddWallet(a, a.Save(Zone.Cryptography, password));

			var path = Path.Combine(Settings.Profile, a.ToString() + "." + WalletExt);

			a.Save(Zone.Cryptography, path, password);

			Log?.Report(this, "Wallet saved", path);

			AccountsChanged?.Invoke();

			return path;
		}

		public void DeleteWallet(AccountAddress a)
		{
			File.Delete(Path.Combine(Settings.Profile, a.ToString() + "." + WalletExt));

			Accounts.Remove(a);

			AccountsChanged?.Invoke();
		}
	}
}
