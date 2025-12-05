
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

	WalletAccount Account;

	public Authentication()
	{
	}

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

	public byte[] Heshify(AccountAddress account)
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		w.Write(account);
		w.WriteUtf8(Application);
		w.WriteUtf8(Net);
		w.WriteBytes(Session);

		return Cryptography.Hash(s.ToArray());
	}
}

public class WalletAccount : IBinarySerializable
{
	public string				Name { get; set; } 
	public AccountAddress		Address { get; set; }
	public AccountKey			Key;
	public List<Authentication>	Authentications = [];
	public Wallet				Wallet;

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

	public Authentication AddAuthentication(string application, string net, byte[] logo, Trust trust)
	{
		if(application == null)
			throw new VaultException(VaultError.IncorrectArgumets);

		if(net == null)
			throw new VaultException(VaultError.IncorrectArgumets);

		//var a = FindAuthentication(net, application);
		//
		//if(a != null)
		//	return a;
		
		var s = new byte[32];
	
		Cryptography.Random.NextBytes(s);
	
		var a = new Authentication
				{
					Application = application, 
					Logo = logo, 
					Net = net, 
					Session = s, 
					Trust = trust
				};
			
		Authentications.Add(a);
		Wallet.AuthenticationHashes.Add(a.Heshify(Address));

		Wallet.Save();
	
		return a;
	}

	//public Authentication FindAuthentication(string net, string application)
	//{
	//	return Authentications.Find(i => i.Net == net && i.Application == application);
	//}

	public void RemoveAuthentication(Authentication authentication)
	{
		Authentications.Remove(authentication);
		var h = authentication.Heshify(Address);
		Wallet.AuthenticationHashes.RemoveAll(i => i.SequenceEqual(h));

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
	public string				Name {get;set; }
	public List<WalletAccount>	Accounts = new();
	public List<byte[]>			AuthenticationHashes = new();
	public byte[]				Encrypted;
	public string				Password;
	public Vault				Vault;

	public bool					Locked => Encrypted != null;
	string						Path => System.IO.Path.Combine(Vault.Settings.Profile, Name + "." + Vault.WalletExtention);

	public const string			Default = "default";

	public Wallet(Vault vault, string name, byte[] data)
	{
		Name = name ?? Default;
		Vault = vault;

		var r = new BinaryReader(new MemoryStream(data));
		
		AuthenticationHashes = r.ReadList(() => r.ReadBytes(Cryptography.HashSize));
		Encrypted			 = r.ReadBytes();
	}

	public Wallet(Vault vault, string name, AccountKey[] keys, string password)
	{
		Name = name ?? Default;
		Vault = vault;
		Password = password;
		Accounts = keys.Select(i => new WalletAccount(this, i)).ToList();
	}

	byte[] Encrypt()
	{
		if(Encrypted != null)
			throw new VaultException(VaultError.Locked);

		var es = new MemoryStream();
		var ew = new BinaryWriter(es);

		ew.Write(Accounts);

		return Vault.Encrypt(es.ToArray(), Password);
	}

	public byte[] ToRaw()
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		w.Write(AuthenticationHashes, w.Write);
		w.WriteBytes(Encrypted ?? Encrypt());

		return s.ToArray();
	}

	internal void Save()
	{
		File.WriteAllBytes(Path, ToRaw());
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

	//public void Access(object uiparent)
	//{
	//	if(Locked)
	//	{	
	//		Vault.UnlockRequested?.Invoke(uiparent, Name); 
	//	}
	//}

	public void Unlock(string password)
	{
		if(Encrypted == null)
			return;

		Password = password;

		var de = Vault.Decrypt(Encrypted, password);

		var r = new BinaryReader(new MemoryStream(de));

		Accounts = r.ReadList(() => { var a = new WalletAccount(this); a.Read(r); return a; });

		Encrypted = null;
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
