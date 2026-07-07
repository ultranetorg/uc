using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Uccs.Nexus;

public class Vault
{
	public const string																	WalletExtention = "uwa";
	public const string																	PrivateKeyExtention = "pk";
	//public static string																WalletExt(Cryptography c) => c is McvCryptography ? WalletExtention : PrivateKeyExtention;

	Flow																				Flow;
	public List<Wallet>																	Wallets = new();
	public IEnumerable<WalletAccount>													UnlockedAccounts => Wallets.SelectMany(i => i.Accounts);
	public VaultSettings																Settings;
	public Zone																			Zone;
	public Cryptography																	Cryptography;
	internal VaultApiServer																ApiServer;
	public IPasswordAsker																PasswordAsker = new ConsolePasswordAsker();

	public Func<string, byte[], string, string, AccountAddress, AuthenticationChoice>	AuthenticationRequested;
	public Func<AccountAddress, Authentication, string, bool>							AuthorizationRequested;
	public Action<object, string>														UnlockRequested;

	public readonly static string[]		PasswordWarning =  {"There is no way to recover Ultranet Account passwords. Back it up in some reliable location.",
															"Make it long. This is the most critical factor. Choose nothing shorter than 15 characters, more if possible.",
															"Use a mix of characters. The more you mix up letters (upper-case and lower-case), numbers, and symbols, the more potent your password is, and the harder it is for a brute force attack to crack it.",
															"Avoid common substitutions. Password crackers are hip to the usual substitutions. Whether you use DOORBELL or D00R8377, the brute force attacker will crack it with equal ease.",
															"Don't use memorable keyboard paths. Much like the advice above not to use sequential letters and numbers, do not use sequential keyboard paths either (like qwerty)."};


	public Vault(string profile, Zone zone, VaultSettings settings, Flow flow)
	{
		Zone			= zone;
		Cryptography	= Cryptography.ByZone(zone);
		Settings		= settings;
		Flow			= flow;
		
		Directory.CreateDirectory(Settings.Profile);

		if(Directory.Exists(Settings.Profile))
		{
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + WalletExtention))
			{
				Wallets.Add(new Wallet(this, Path.GetFileNameWithoutExtension(i), File.ReadAllBytes(i)));
			}
		}

		if(Settings.Api != null)
		{
			RunApi();
		}
	}

	public Vault(Zone zone, VaultSettings settings, Flow flow)
	{
		Zone			= zone;
		Cryptography	= Cryptography.ByZone(zone);
		Settings		= settings;
		Flow			= flow;
	}
	
	public void Stop()
	{
		Flow.Abort();

		ApiServer?.Stop();
	}
	
	public void RunApi()
	{
		if(ApiServer != null)
			throw new NodeException(NodeError.AlreadyRunning);

		ApiServer = new VaultApiServer(this, Settings.Api, Flow);
	}

	public WalletAccount Find(AccountAddress address)
	{
		foreach(var i in Wallets)
			foreach(var j in i.Accounts)
				if(j.Address == address)
					return j;

		return null;
	}

	public WalletAccount Find(string name)
	{
		foreach(var i in Wallets)
			foreach(var j in i.Accounts)
				if(j.Name == name)
					return j;

		return null;
	}

	public Wallet FindWallet(string name)
	{
		return Wallets.Find(i => string.Compare(i.Name, name ?? Wallet.Default, true) == 0);
	}

	public Wallet CreateWallet(string name, string password, int accounts)
	{
		var w = new Wallet(this, name, Enumerable.Range(0, accounts).ToDictionary(i => AccountKey.Create(), i => (string)null), password);
		return w;
	}

	public Wallet CreateWallet(string name, IDictionary<AccountKey, string> keys, string password)
	{
		var w = new Wallet(this, name, keys, password);
		return w;
	}

	public Wallet CreateWallet(string name, byte[] raw)
	{
		var w = new Wallet(this, name, raw);
		return w;
	}

	public void AddWallet(string name, byte[] raw)
	{
		if(FindWallet(name) != null)
			throw new VaultException(VaultError.AlreadyExists);

		var w = new Wallet(this, name, raw);

		Wallets.Add(w);
		
		w.Save();
	}

	public Wallet AddWallet(string name, IDictionary<AccountKey, string> keys, string password)
	{
		if(FindWallet(name) != null)
			throw new VaultException(VaultError.AlreadyExists);

		var w = CreateWallet(name, keys, password);
		
		w.Password = password;

		w.Save();

		Wallets.Add(w);

		return w;
	}

	public bool IsUnlocked(AccountAddress address)
	{
		return Find(address)?.Key != null;
	}

	public void DeleteWallet(string name)
	{
		name = (name ?? Wallet.Default);

		var w = Wallets.FirstOrDefault(i => i.Name == name);
		
		if(w == null)
			throw new VaultException(VaultError.NotFound);

		File.Delete(Path.Combine(Settings.Profile, name + "." + WalletExtention));

		Wallets.Remove(w);
	}

	public bool IsAuthenticated(string user, string application, string net, byte[] session)
	{
		var h = new Authentication {User = user, Application = application, Net = net, Session = session}.Hashify();

		return Wallets.Any(i => i.AuthenticationHashes.Contains(h, Bytes.EqualityComparer));
	}

	public byte[] Encrypt(byte[] data, string password)
	{
        byte[] ps = RandomNumberGenerator.GetBytes(32);
        byte[] iv = RandomNumberGenerator.GetBytes(16);

		using(Aes aesAlg = Aes.Create())
		{
			aesAlg.Key = Cryptography.HashifyPassword(password, ps);
			aesAlg.IV = iv;

			var e = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
			
			byte[] en;
			
			using(var msEncrypt = new MemoryStream())
			{
				using(var es = new CryptoStream(msEncrypt, e, CryptoStreamMode.Write))
				{
					es.Write(data, 0, data.Length);
				}
				en = msEncrypt.ToArray();
			}

			var s = new MemoryStream();
			var w = new Writer(s);
			
			w.Write(1);
			w.WriteBytes(ps);
			w.WriteBytes(iv);
			w.WriteBytes(en);

			return s.ToArray();
		}
	}

	public byte[] Decrypt(byte[] data, string password)
	{
		var r = new Reader(data);
			
		var v  = r.ReadInt32();

		if(v != 1)
			throw new VaultException(VaultError.NotSupported);

		var ps = r.ReadBytes();
		var iv = r.ReadBytes();
		var en = r.ReadBytes();

		using(Aes aesAlg = Aes.Create())
		{
			aesAlg.Key = Cryptography.HashifyPassword(password, ps);
			aesAlg.IV = iv;

			ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
			byte[] de;

			using(var msDecrypt = new MemoryStream(en))
			{
				using(var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
				{
					using(var msPlain = new MemoryStream())
					{
						csDecrypt.CopyTo(msPlain);
						de = msPlain.ToArray();
					}
				}
			}

			return de;
		}
	}

	public AuthenticationResult Authenticate(string application, string net, string user, byte[] logo, AccountAddress account, Flow flow)
	{
		var c = AuthenticationRequested?.Invoke(application, logo, net, user, account);
	
		if(c != null)
		{
			var a = Find(c.Account);
		
			if(a == null)
				throw new VaultException(VaultError.AccountNotFound);
		
			var n = a.AddAuthentication(application, net, user, logo, c.Trust);
		
			return new AuthenticationResult {Signer = c.Account, Session = n.Session};
		} 
		else
			throw new VaultException(VaultError.Rejected);
	}

	public byte[] Authorize(CryptographyType cryptography, string net, string operation, string user, byte[] session, byte[] Hash, Flow flow)
	{
		if(string.IsNullOrWhiteSpace(net) || session.Length != Cryptography.HashLength)
			throw new VaultException(VaultError.IncorrectArgumets);

		var h = new	Authentication {Net = net, User = user, Session = session}.Hashify();

		WalletAccount acc;

		var w = Wallets.Find(i => i.AuthenticationHashes.Contains(h, Bytes.EqualityComparer));
	
		if(w == null)
			throw new VaultException(VaultError.NotAuthorized);
	
		if(w.Locked)
			UnlockRequested?.Invoke(null,w.Name);
	
		if(w.Locked)
			throw new VaultException(VaultError.Locked);
	
		//acc = w.Accounts.Find(i => i.Address == Account);
			
		acc = w.Accounts.FirstOrDefault(i => i.Authentications.Any(i =>	i.Session.SequenceEqual(session)));
	
		var au = acc?.Authentications.Find(i => i.Session.SequenceEqual(session));

		if(au == null)
			throw new VaultException(VaultError.Corrupted);
	
		if(au.Trust == Trust.AskEveryTime)
			AuthorizationRequested(acc.Address, au, operation);

		return cryptography switch 
							{
								CryptographyType.No		=> Cryptography.No.Sign(acc.Key, Hash),
								CryptographyType.Mcv	=> Cryptography.Mcv.Sign(acc.Key, Hash),
								CryptographyType.Iccp	=> Cryptography.Iccp.Sign(acc.Key, Hash),
								_ => throw new VaultException(VaultError.UnknownCtyptography)
							};
	}
}
