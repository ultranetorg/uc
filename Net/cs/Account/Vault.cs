namespace Uccs.Net;

public enum Trust
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

public class Authentication : IBinarySerializable
{
	public string	Net { get; set; }
	public byte[]	Session { get; set; }
	public Trust	Trust { get; set; }

	public void Read(BinaryReader reader)
	{
		Net = reader.ReadString();
		Trust = reader.ReadEnum<Trust>();
		
		if(Trust != Trust.None)
		{
			Session = reader.ReadBytes();
		}
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Net);
		writer.WriteEnum(Trust);
		
		if(Trust != Trust.None)
		{
			writer.WriteBytes(Session);
		}
	}
}

public class Wallet
{
	public AccountAddress		Address; 
	public AccountKey			Key;
	public List<Authentication>	Authentications = [];
	public byte[]				Encrypted;
	Vault						Vault;

	public Wallet(Vault vault)
	{
		Vault = vault;
	}

	public Wallet(Vault vault, AccountKey key)
	{
		Vault = vault;
		Address = key;
		Key = key;
	}

	public Wallet(Vault vault, byte[] raw)
	{
		Vault = vault;

		var r = new BinaryReader(new MemoryStream(raw));

		Address		= r.ReadAccount();
		Encrypted	= r.ReadBytes();
	}

	public byte[] ToRaw()
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		w.Write(Address);
		w.WriteBytes(Encrypted);

		return s.ToArray();
	}

	public void Encrypt(string password)
	{
		var es = new MemoryStream();
		var ew = new BinaryWriter(es);

		ew.Write(Key.GetPrivateKeyAsBytes());
		ew.Write(Authentications);

		Encrypted = Vault.Cryptography.Encrypt(es.ToArray(), password);
	}

	public byte[] GetSession(string net, Trust trust)
	{
		var a = Authentications.Find(i => i.Net == net);
			
		if(a == null)
		{
			var s = new byte[32];
	
			Cryptography.Random.NextBytes(s);
	
			Authentications.Add(new Authentication {Net = net, Session = s, Trust = trust});
	
			return s;
		} 
		else
		{
			return a.Session;
		}
	}

	public Authentication FindAuthentication(string net)
	{
		return Authentications.Find(i => i.Net == net);
	}

	public void Unlock(string password)
	{
		var de = Vault.Cryptography.Decrypt(Encrypted, password);

		var r = new BinaryReader(new MemoryStream(de));

		Key				= new AccountKey(r.ReadBytes(Cryptography.PrivateKeyLength));
		Authentications	= r.ReadList<Authentication>();
	}

	public void Save(string path)
	{
		File.WriteAllBytes(path, ToRaw());
	}
}

public class Vault
{
	public const string					EncryptedWalletExtention = "uwa";
	public const string					PrivakeKeyWalletExtention = "uwpk";
	public static string				WalletExt(Cryptography c) => c is NormalCryptography ? EncryptedWalletExtention : PrivakeKeyWalletExtention;

	string								Profile;
	public List<Wallet>					Wallets = new();
	public Cryptography					Cryptography;

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
				Wallets.Add(new Wallet(this, File.ReadAllBytes(i)));
			}
		}
	}

	public Wallet Find(AccountAddress address)
	{
		return Wallets.Find(i => i.Address == address);
	}

	public Wallet CreateWallet()
	{
		return new Wallet(this, AccountKey.Create());
	}

	public Wallet CreateWallet(AccountKey key)
	{
		return new Wallet(this, key);
	}

	public Wallet CreateWallet(byte[] raw)
	{
		return new Wallet(this, raw);
	}

	public void AddWallet(AccountKey key)
	{
		Wallets.Add(CreateWallet(key));
	}

	public void AddWallet(byte[] raw)
	{
		var w = new Wallet(this, raw);

		if(Wallets.Any(i => i.Address == w.Address))
			throw new VaultException("Account with such key already exists");
		
		Wallets.Add(w);
		
		SaveWallet(w.Address);
	}

	public void AddWallet(AccountKey key, string password)
	{
		if(Wallets.Any(i => i.Key == key))
			throw new VaultException("Account with such key already exists");

		var w = new Wallet(this) {Address = key, Key = key};
		
		Wallets.Add(w);

		w.Encrypt(password);

		SaveWallet(w.Address);
	}

	public AccountKey Unlock(AccountAddress address, string password)
	{
		var w = Find(address) 
				??
				throw new VaultException("Account not found");

		if(w.Key != null)
			return w.Key;

		w.Unlock(password);

		return w.Key;
	}

	public bool IsUnlocked(AccountAddress address)
	{
		return Find(address)?.Key != null;
	}

	public string SaveWallet(AccountAddress address)
	{
		var w = Find(address) 
				??
				throw new VaultException("Account not found");

		var path = Path.Combine(Profile, w.Address.ToString() + "." + WalletExt(Cryptography));

		w.Save(path);

		return path;
	}

	public void DeleteWallet(AccountAddress account)
	{
		File.Delete(Path.Combine(Profile, account.ToString() + "." + WalletExt(Cryptography)));

		Wallets.RemoveAll(i => i.Address == account);
	}
}
