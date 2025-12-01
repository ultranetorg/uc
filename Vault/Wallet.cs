
namespace Uccs.Vault;

public class AuthenticationChoice
{
	public AccountAddress	Account { get; set; }
	public Trust			Trust { get; set; }
}

public class Authentication : IBinarySerializable
{
	public string	Application { get; set; }
	public byte[]	Logo { get; set; }
	public string	Net { get; set; }
	public Trust	Trust { get; set; }
	public byte[]	Session { get; set; }

	public void Read(BinaryReader reader)
	{
		Application = reader.ReadUtf8();
		Logo		= reader.ReadBytes();
		Net			= reader.ReadUtf8();
		Trust		= reader.Read<Trust>();
		Session		= reader.ReadBytes();
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Application);
		writer.WriteBytes(Logo);
		writer.WriteUtf8(Net);
		writer.Write(Trust);
		writer.WriteBytes(Session);
	}
}

public class WalletAccount : IBinarySerializable
{
	public string				Name { get; set; } 
	public AccountAddress		Address { get; set; }
	public AccountKey			Key;
	public List<Authentication>	Authentications = [];
	Wallet						Wallet;

	public WalletAccount()
	{ 
	}

	public WalletAccount(Wallet vault)
	{
		Wallet = vault;
	}

	public WalletAccount(Wallet vault, AccountKey key)
	{
		Wallet = vault;
		Address = key;
		Key = key;
	}

	public override string ToString()
	{
		return $"{Address}";
	}

	public Authentication GetAuthentication(string application, byte[] logo, string net, Trust trust)
	{
		if(application == null)
			throw new VaultException(VaultError.IncorrectArgumets);

		if(net == null)
			throw new VaultException(VaultError.IncorrectArgumets);

		var a = Authentications.Find(i => i.Application == application && i.Net == net);
		
		if(a != null)
			return a;
		
		var s = new byte[32];
	
		Cryptography.Random.NextBytes(s);
	
		a = new Authentication {Application = application, Logo = logo, Net = net, Session = s, Trust = trust};

		Authentications.Add(a);
	
		return a;
	}

	public Authentication FindAuthentication(string net)
	{
		return Authentications.Find(i => i.Net == net);
	}

	public void RemoveAuthentication(Authentication authentication)
	{
		Authentications.Remove(authentication);

		Wallet.Save();
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteUtf8Nullable(Name);
		writer.Write(Key.PrivateKey);
		writer.Write(Authentications);
	}

	public void Read(BinaryReader reader)
	{
		Name			= reader.ReadUtf8Nullable();
		Key				= new AccountKey(reader.ReadBytes(Cryptography.PrivateKeyLength));
		Authentications	= reader.ReadList<Authentication>();

		Address	= Key;
	}
}

public class Wallet
{
	public string				Name;
	public List<WalletAccount>	Accounts = new();
	public byte[]				Encrypted;
	public string				Password;
	Vault						Vault;

	public bool					Locked => Encrypted != null;

	string						Path => System.IO.Path.Combine(Vault.Settings.Profile, Name + "." + Vault.WalletExt(Vault.Cryptography));

	public const string			Default = "default";

	public byte[] Encrypt()
	{
		if(Encrypted != null)
			throw new VaultException(VaultError.Locked);

		var es = new MemoryStream();
		var ew = new BinaryWriter(es);

		ew.Write(Accounts);

		return Vault.Cryptography.Encrypt(es.ToArray(), Password);
	}

	public Wallet(Vault vault, string name, byte[] encrypted)
	{
		Name = name ?? Default;
		Vault = vault;
		Encrypted = encrypted;
	}

	public Wallet(Vault vault, string name, AccountKey[] keys, string password)
	{
		Name = name ?? Default;
		Vault = vault;
		Password = password;
		Accounts = keys.Select(i => new WalletAccount(this, i)).ToList();
	}

	public WalletAccount AddAccount(byte[] key)
	{
		if(Encrypted != null)
			throw new VaultException(VaultError.Locked);

		if(key != null && Accounts.Any(i => Bytes.Comparer.Compare(i.Key.PrivateKey, key) == 0))
			throw new VaultException(VaultError.AlreadyExists);

		var a = new WalletAccount(this, key == null ? AccountKey.Create() : new AccountKey(key));
		
		Accounts.Add(a);

		Save();

		return a;
	}

	public void DeleteAccount(WalletAccount account)
	{
		Accounts.Remove(account);
	
		Save();
	}

	public void Lock()
	{
		if(Encrypted != null)
			return;

		Encrypted = Encrypt();

		Accounts.Clear();
		Password = null;
	}

	public void Unlock(string password)
	{
		if(Encrypted == null)
			return;

		Password = password;

		var de = Vault.Cryptography.Decrypt(Encrypted, password);

		var r = new BinaryReader(new MemoryStream(de));

		Accounts = r.ReadList(() => { var a = new WalletAccount(this); a.Read(r); return a; });

		Encrypted = null;
	}

	public void Save()
	{
		File.WriteAllBytes(Path, Encrypted ?? Encrypt());
	}

	public void Rename(string name)
	{
		if(string.Compare(Name, name, true) == 0)
			return;

		if(Vault.FindWallet(name) != null)
			throw new VaultException(VaultError.AlreadyExists);

		var old = Path;

		Name = name;

		if(File.Exists(Path))
			throw new VaultException(VaultError.AlreadyExists);

		Save();

		File.Delete(old);
	}
}
