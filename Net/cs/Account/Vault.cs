namespace Uccs.Net
{
	public class Vault
	{
		public const string							EncryptedWalletExtention = "uwa";
		public const string							PrivakeKeyWalletExtention = "uwpk";
		public static string						WalletExt(Cryptography c) => c is NormalCryptography ? EncryptedWalletExtention : PrivakeKeyWalletExtention;

		string										Profile;
		public Dictionary<AccountAddress, byte[]>	Wallets = new();
		public List<AccountKey>						Keys = new();
		public Cryptography							Cryptography;

		public event Action							AccountsChanged;						

		public readonly static string[]				PasswordWarning = {	"There is no way to recover Ultranet Account passwords. Back it up in some reliable location.",
																		"Make it long. This is the most critical factor. Choose nothing shorter than 15 characters, more if possible.",
																		"Use a mix of characters. The more you mix up letters (upper-case and lower-case), numbers, and symbols, the more potent your password is, and the harder it is for a brute force attack to crack it.",
																		"Avoid common substitutions. Password crackers are hip to the usual substitutions. Whether you use DOORBELL or D00R8377, the brute force attacker will crack it with equal ease.",
																		"Don’t use memorable keyboard paths. Much like the advice above not to use sequential letters and numbers, do not use sequential keyboard paths either (like qwerty)."};

		public Vault(string profile, bool encrypt)
		{
			Profile = profile;
			Cryptography = encrypt ? new NormalCryptography() : new NoCryptography() ;

			Directory.CreateDirectory(profile);

			if(Directory.Exists(profile))
			{
				foreach(var i in Directory.EnumerateFiles(profile, "*." + WalletExt(Cryptography)))
				{
					Wallets[AccountAddress.Parse(Path.GetFileNameWithoutExtension(i))] = File.ReadAllBytes(i);
				}
			}
		}

		public void AddKey(AccountKey key)
		{
			Keys.Add(key);
		}

		public void AddWallet(byte[] wallet)
		{
			var a  = Cryptography.AccountFromWallet(wallet);

			Wallets[a] = wallet;
		}

		public void AddWallet(AccountKey key, string password)
		{
			Wallets[key] = key.Save(Cryptography, password);
		}

		public AccountKey AddWallet(byte[] privatekey, string password)
		{
			if(privatekey.Length != Cryptography.PrivateKeyLength)
				throw new ArgumentException();

			var k = new AccountKey(privatekey);

			if(Wallets.ContainsKey(k))
				throw new ArgumentException();

			Wallets[k] = k.Save(Cryptography, password);

			return k;
		}

		public AccountKey Unlock(AccountAddress a, string password)
		{
			if(Keys.Contains(a))
				return Keys.Find(i => i == a);

			var p = AccountKey.Load(Cryptography, Wallets[a], password);

			Keys.Add(p);

			//var i = Accounts.IndexOf(a);
			//Accounts.Remove(a);
			//Accounts.Insert(i, p);

			//Log?.Report(this, "Wallet unlocked", a.ToString());

			return p;
		}

		public bool IsUnlocked(AccountAddress account)
		{
			return Keys.Any(i => i == account);
		}

		public AccountKey GetKey(AccountAddress account)
		{
			return Keys.First(i => i == account);
		}

		public string SaveWallet(AccountAddress account)
		{
			var path = Path.Combine(Profile, account.ToString() + "." + WalletExt(Cryptography));

			File.WriteAllBytes(path, Wallets[account]);

			AccountsChanged?.Invoke();

			return path;
		}

		public void DeleteWallet(AccountAddress account)
		{
			File.Delete(Path.Combine(Profile, account.ToString() + "." + WalletExt(Cryptography)));

			Keys.RemoveAll(i => i == account);

			AccountsChanged?.Invoke();
		}
	}
}
