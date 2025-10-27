namespace Uccs.Vault;

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

	public override string ToString()
	{
		return $"{Address} Authentications={Authentications.Count}";
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
		var path = Path.Combine(Vault.Settings.Profile, Name + "." + Vault.WalletExt(Vault.Cryptography));

		File.WriteAllBytes(path, RawLoaded ?? Raw);
	}
}
