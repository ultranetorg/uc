using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Model;
using Nethereum.Signer;
using Org.BouncyCastle.Crypto;

namespace UC.Net
{
	public class Vault
	{
		public const string					EthereumWalletExtention = "uwe";
		public const string					NoCryptoWalletExtention = "uwnc";
		public string						WalletExt => Cryptography.Current is EthereumCryptography ? EthereumWalletExtention : NoCryptoWalletExtention;

		Settings							Settings;
		Log									Log;
		public Dictionary<Account, byte[]>	Wallets = new();
		public List<Account>				Accounts = new();
		public Dictionary<Account, int>		OperationIds = new();
 		List<PrivateAccount>				Fathers = new();

		public event Action					AccountsChanged;						

		public readonly static string[]		PasswordWarning = {	"There is no way to recover Ultranet Account passwords. Back it up in some reliable location.",
																"Make it long. This is the most critical factor. Choose nothing shorter than 15 characters, more if possible.",
																"Use a mix of characters. The more you mix up letters (upper-case and lower-case), numbers, and symbols, the more potent your password is, and the harder it is for a brute force attack to crack it.",
																"Avoid common substitutions. Password crackers are hip to the usual substitutions. Whether you use DOORBELL or D00R8377, the brute force attacker will crack it with equal ease.",
																"Don’t use memorable keyboard paths. Much like the advice above not to use sequential letters and numbers, do not use sequential keyboard paths either (like qwerty)."};

		public Vault(Settings settings, Log log)
		{
			Settings = settings;
			Log		 = log;

			Directory.CreateDirectory(Settings.Profile);

			if(Settings.Accounts != null)
			{
				foreach(var i in Settings.Accounts)
				{
					Wallets[Account.Parse(Path.GetFileNameWithoutExtension(i))] = File.ReadAllBytes(i);
					Accounts.Add(Account.Parse(Path.GetFileNameWithoutExtension(i)));
				}
			}
			else
			{
				if(Directory.Exists(Settings.Profile))
				{
					foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + WalletExt))
					{
						Wallets[Account.Parse(Path.GetFileNameWithoutExtension(i))] = File.ReadAllBytes(i);
						Accounts.Add(Account.Parse(Path.GetFileNameWithoutExtension(i)));
					}
				}
			}

		}

		public void AddWallet(Account account, byte[] wallet)
		{
			Accounts.Add(account);
			Wallets[account] = wallet;
			
			Log?.Report(this, "Wallet added", account.ToString());
		}

		public PrivateAccount Unlock(Account a, string password)
		{
			var p = PrivateAccount.Load(Wallets[a], password);

			Accounts.Remove(a);
			Accounts.Add(p);

			Log?.Report(this, "Account unlocked", a.ToString());

			return p;
		}

		public PrivateAccount GetPrivate(Account a)
		{
			return Accounts.Find(i => i == a) as PrivateAccount;
		}

		public PrivateAccount GetFather(Account a)
		{
			var f = Fathers.Find(i => i == a);

			if(f != null)
				return f;

			f = new PrivateAccount(new EthECKey(File.ReadAllBytes(Path.Join(Settings.Secret.Fathers, a + "." + Vault.NoCryptoWalletExtention)), true));

			Fathers.Add(f);

			return f;
		}

		public string SaveAccount(PrivateAccount a, string password)
		{
			AddWallet(a, a.Save(password));

			var path = Path.Combine(Settings.Profile, a.ToString() + "." + WalletExt);

			a.Save(path, password);

			Log?.Report(this, "Wallet saved", path);

			AccountsChanged?.Invoke();

			return path;
		}

		public void DeleteAccount(Account a)
		{
			File.Delete(Path.Combine(Settings.Profile, a.ToString() + "." + WalletExt));

			Accounts.Remove(a);

			AccountsChanged?.Invoke();
		}
	}
}
