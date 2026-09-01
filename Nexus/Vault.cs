using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Uccs.Nexus;

public class Vault
{
	public const string																WalletExtention = "uwa";
	public const string																PrivateKeyExtention = "pk";
	//public static string															WalletExt(Cryptography c) => c is McvCryptography ? WalletExtention : PrivateKeyExtention;

	Flow																			Flow;
	public List<Wallet>																Wallets = new();
	public IEnumerable<WalletKey>													UnlockedKeys => Wallets.SelectMany(i => i.Keys);
	public VaultSettings															Settings;
	public Zone																		Zone;
	public Cryptography																Cryptography;
	internal VaultApiServer															ApiServer;
	public IPasswordAsker															PasswordAsker = new ConsolePasswordAsker();

	public Func<string, byte[], string, string, PublicKey, AuthenticationChoice>	AuthenticationRequested;
	public Func<PublicKey, Authentication, string, bool>							AuthorizationRequested;
	public Action<object, string>													UnlockRequested;

	public readonly static string[]		PasswordWarning =  {"There is no way to recover Key passwords. Back it up in some reliable location.",
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

	public WalletKey Find(PublicKey address)
	{
		foreach(var i in Wallets)
			foreach(var j in i.Keys)
				if(j.Public == address)
					return j;

		return null;
	}

	public WalletKey Find(string name)
	{
		foreach(var i in Wallets)
			foreach(var j in i.Keys)
				if(j.Alias == name)
					return j;

		return null;
	}

	public Wallet FindWallet(string name)
	{
		return Wallets.Find(i => string.Compare(i.Name, name ?? Wallet.Default, true) == 0);
	}

	public Wallet CreateWallet(string name, string password, int accounts)
	{
		if(string.IsNullOrWhiteSpace(name) || name.Length > 256 || name.Any(i => System.IO.Path.GetInvalidFileNameChars().Contains(i)))
			throw new VaultException(VaultError.InvalidWalletName);

		if(Wallets.Any(i => i.Name == name))
			throw new VaultException(VaultError.AlreadyExists);

		var w = new Wallet(this, name, Enumerable.Range(0, accounts).ToDictionary(i => SecretKey.Create(), i => (string)null), password);

		Wallets.Add(w);

		return w;
	}

	public Wallet CreateWallet(string name, IDictionary<SecretKey, string> keys, string password)
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

	public Wallet AddWallet(string name, IDictionary<SecretKey, string> keys, string password)
	{
		if(FindWallet(name) != null)
			throw new VaultException(VaultError.AlreadyExists);

		var w = CreateWallet(name, keys, password);

		w.Save();

		Wallets.Add(w);

		return w;
	}

	public bool IsUnlocked(PublicKey address)
	{
		return Find(address)?.Secret != null;
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

	public AuthenticationResult Authenticate(string application, string net, string user, byte[] logo, PublicKey key)
	{
		var c = AuthenticationRequested?.Invoke(application, logo, net, user, key);
	
		if(c != null)
		{
			if(c.Waiting)
				return null; 

			var a = Find(c.PublickKey);
		
			if(a == null)
				throw new VaultException(VaultError.NotFound);
		
			var n = a.AddAuthentication(application, net, user, logo, c.Trust);
		
			return new AuthenticationResult {Signer = c.PublickKey, Session = n.Session};
		} 
		else
			throw new VaultException(VaultError.Rejected);
	}

	public AuthorizationResult Authorize(CryptographyType cryptography, string net, string operation, string user, byte[] session, byte[] Hash, Flow flow)
	{
		if(string.IsNullOrWhiteSpace(net) || session.Length != Cryptography.HashLength)
			throw new VaultException(VaultError.IncorrectArgumets);

		var h = new	Authentication {Net = net, User = user, Session = session}.Hashify();

		WalletKey acc;

		var w = Wallets.Find(i => i.AuthenticationHashes.Contains(h, Bytes.EqualityComparer));
	
		if(w == null)
			throw new VaultException(VaultError.NotAuthorized);
	
		if(w.Locked)
			UnlockRequested?.Invoke(null,w.Name);
	
		if(w.Locked)
			throw new VaultException(VaultError.Locked);
	
		//acc = w.Accounts.Find(i => i.Address == Account);
			
		acc = w.Keys.FirstOrDefault(i => i.Authentications.Any(i =>	i.Session.SequenceEqual(session)));
	
		var au = acc?.Authentications.Find(i => i.Session.SequenceEqual(session));

		if(au == null)
			throw new VaultException(VaultError.Corrupted);
	
		if(au.Trust == Trust.AskEveryTime)
			AuthorizationRequested(acc.Public, au, operation);

		return	new AuthorizationResult
				{
					Signer = acc.Public, 
					Signature = cryptography switch 
					{
						CryptographyType.No		=> Cryptography.No.Sign(acc.Secret, Hash, SigningFeatures.None),
						CryptographyType.Mcv	=> Cryptography.Mcv.Sign(acc.Secret, Hash, SigningFeatures.None),
						CryptographyType.Iccp	=> Cryptography.Iccp.Sign(acc.Secret, Hash, SigningFeatures.None),
						_ => throw new VaultException(VaultError.UnknownCtyptography)
					}
				};
	}
}
