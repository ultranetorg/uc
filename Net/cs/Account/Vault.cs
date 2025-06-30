namespace Uccs.Net;

public enum Trust : byte
{
	None,
	NonSpending,
	Spending
}

public class AuthenticationChioce
{
	public AccountAddress	Account { get; set; }
	public Trust			Trust { get; set; }
}

public class AccountSession
{
	public AccountAddress	Account { get; set; }
	public byte[]			Session { get; set; }
}

public class Authentication : IBinarySerializable
{
	public string	Net { get; set; }
	public byte[]	Session { get; set; }
	public Trust	Trust { get; set; }

	public void Read(BinaryReader reader)
	{
		Net = reader.ReadString();
		Trust = reader.Read<Trust>();
		
		if(Trust != Trust.None)
		{
			Session = reader.ReadBytes();
		}
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Net);
		writer.Write(Trust);
		
		if(Trust != Trust.None)
		{
			writer.WriteBytes(Session);
		}
	}
}

public class WalletAccount : IBinarySerializable
{
	public AccountAddress		Address; 
	public AccountKey			Key;
	public List<Authentication>	Authentications = [];
	Vault						Vault;

	public WalletAccount(Vault vault)
	{
		Vault = vault;
	}

	public WalletAccount(Vault vault, AccountKey key)
	{
		Vault = vault;
		Address = key;
		Key = key;
	}

	public Authentication GetAuthentication(string net, Trust trust)
	{
		var a = Authentications.Find(i => i.Net == net);
		
		if(a != null)
			return a;
		
		var s = new byte[32];
	
		Cryptography.Random.NextBytes(s);
	
		a = new Authentication {Net = net, Session = s, Trust = trust};

		Authentications.Add(a);
	
		return a;
	}

	public Authentication FindAuthentication(string net)
	{
		return Authentications.Find(i => i.Net == net);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Key.PrivateKey);
		writer.Write(Authentications);
	}

	public void Read(BinaryReader reader)
	{
		Key				= new AccountKey(reader.ReadBytes(Cryptography.PrivateKeyLength));
		Authentications	= reader.ReadList<Authentication>();

		Address	= Key;
	}
}

public class Wallet
{
	public string				Name;
	public List<WalletAccount>	Accounts = new();
	public byte[]				RawLoaded;
	public string				Password;
	Vault						Vault;

	public bool					Locked => RawLoaded != null;

	public byte[] Raw
	{
		get
		{
			if(RawLoaded != null)
				throw new VaultException(VaultError.Locked);

			var es = new MemoryStream();
			var ew = new BinaryWriter(es);

			ew.Write(Accounts);

			return Vault.Cryptography.Encrypt(es.ToArray(), Password);
		}
	}

	public Wallet(Vault vault, string name, byte[] raw)
	{
		Name = name;
		Vault = vault;
		RawLoaded = raw;
	}

	public Wallet(Vault vault, string name, AccountKey[] keys, string password)
	{
		Name = name;
		Vault = vault;
		Password = password;
		Accounts = keys.Select(i => new WalletAccount(Vault, i)).ToList();
	}

	public WalletAccount AddAccount(byte[] key)
	{
		if(RawLoaded != null)
			throw new VaultException(VaultError.Locked);

		var a = new WalletAccount(Vault, key == null ? AccountKey.Create() : new AccountKey(key));
		
		Accounts.Add(a);

		Save();

		return a;
	}

	public void Lock()
	{
		if(RawLoaded != null)
			return;

		RawLoaded = Raw;

		Accounts.Clear();
		Password = null;
	}

	public void Unlock(string password)
	{
		if(RawLoaded == null)
			return;

		Password = password;

		var de = Vault.Cryptography.Decrypt(RawLoaded, password);

		var r = new BinaryReader(new MemoryStream(de));

		Accounts = r.ReadList(() => { var a = new WalletAccount(Vault); a.Read(r); return a; });

		RawLoaded = null;
	}

	public void Save()
	{
		var path = Path.Combine(Vault.Profile, Name + "." + Vault.WalletExt(Vault.Cryptography));

		File.WriteAllBytes(path, RawLoaded ?? Raw);
	}
}

public class Vault
{
	public const string					WalletExtention = "uwa";
	public const string					PrivateKeyExtention = "pk";
	public static string				WalletExt(Cryptography c) => c is NormalCryptography ? WalletExtention : PrivateKeyExtention;

	public string						Profile;
	public List<Wallet>					Wallets = new();
	public Cryptography					Cryptography;

	public IEnumerable<WalletAccount>	UnlockedAccounts => Wallets.SelectMany(i => i.Accounts);

	public readonly static string[]		PasswordWarning =  {"There is no way to recover Ultranet Account passwords. Back it up in some reliable location.",
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
				Wallets.Add(new Wallet(this, Path.GetFileName(i), File.ReadAllBytes(i)));
			}
		}
	}

	public WalletAccount Find(AccountAddress address)
	{
		foreach(var i in Wallets)
			foreach(var j in i.Accounts)
				if(j.Address == address)
					return j;

		return null;
	}

	public Wallet CreateWallet(string password, int accounts = 1)
	{
		var w = new Wallet(this, Wallets.Count.ToString(), Enumerable.Range(0, accounts).Select(i => AccountKey.Create()).ToArray(), password);
		return w;
	}

	public Wallet CreateWallet(AccountKey[] key, string password)
	{
		var w = new Wallet(this, Wallets.Count.ToString(), key, password);
		return w;
	}

	public Wallet CreateWallet(byte[] raw)
	{
		var w = new Wallet(this, Wallets.Count.ToString(), raw);
		return w;
	}

	public void AddWallet(byte[] raw)
	{
		var w = new Wallet(this, Wallets.Count.ToString(), raw);

		Wallets.Add(w);
		
		w.Save();
	}

	public Wallet AddWallet(AccountKey[] key, string password)
	{
		var w = CreateWallet(key, password);
		
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
		File.Delete(Path.Combine(Profile, name + "." + WalletExt(Cryptography)));

		Wallets.RemoveAll(i => i.Name == name);
	}
}
