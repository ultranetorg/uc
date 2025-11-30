using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace Uccs.Vault;

public class Vault : Cli
{
	public const string					WalletExtention = "uwa";
	public const string					PrivateKeyExtention = "pk";
	public static string				WalletExt(Cryptography c) => c is NormalCryptography ? WalletExtention : PrivateKeyExtention;

	public List<Wallet>					Wallets = new();
	public IEnumerable<WalletAccount>	UnlockedAccounts => Wallets.SelectMany(i => i.Accounts);
	public Cryptography					Cryptography;
	public VaultSettings				Settings;
	internal VaultApiServer				ApiServer;
	public IPasswordAsker				PasswordAsker = new ConsolePasswordAsker();

	public Func<string, string, AccountAddress, AuthenticationChoice>	AuthenticationRequested;
	public Func<string, string, AccountAddress, string, bool>			AuthorizationRequested;
	public Action<AccountAddress>										UnlockRequested;

	public readonly static string[]		PasswordWarning =  {"There is no way to recover Ultranet Account passwords. Back it up in some reliable location.",
															"Make it long. This is the most critical factor. Choose nothing shorter than 15 characters, more if possible.",
															"Use a mix of characters. The more you mix up letters (upper-case and lower-case), numbers, and symbols, the more potent your password is, and the harder it is for a brute force attack to crack it.",
															"Avoid common substitutions. Password crackers are hip to the usual substitutions. Whether you use DOORBELL or D00R8377, the brute force attacker will crack it with equal ease.",
															"Don't use memorable keyboard paths. Much like the advice above not to use sequential letters and numbers, do not use sequential keyboard paths either (like qwerty)."};

	static Vault()
	{
	}

	static void Main(string[] args)
	{
		var Boot = new NetBoot(ExeDirectory);
		var u = new Vault(Boot.Profile, Boot.Zone, null, new Flow(nameof(Vault), new Log()));

		u.Execute(Boot);

		u.Stop();
	}

	public Vault(string profile, Zone zone, VaultSettings settings, Flow flow)
	{
		Settings = settings ?? new VaultSettings(profile, zone);
		Flow = flow;
		Cryptography = Settings.Encrypt ? new NormalCryptography() : new NoCryptography() ;

		Directory.CreateDirectory(Settings.Profile);

		if(Directory.Exists(Settings.Profile))
		{
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + WalletExt(Cryptography)))
			{
				Wallets.Add(new Wallet(this, Path.GetFileName(i), File.ReadAllBytes(i)));
			}
		}

		RunApi();
	}
	
	public void Stop()
	{
		Flow.Abort();

		ApiServer?.Stop();
	}
	
	public void RunApi()
	{
		if(!HttpListener.IsSupported)
		{
			Environment.ExitCode = -1;
			throw new RequirementException("Windows XP SP2, Windows Server 2003 or higher is required to use the application.");
		}

		if(ApiServer != null)
			throw new NodeException(NodeError.AlreadyRunning);

		ApiServer = new VaultApiServer(this, Settings.Api, Flow);
	}

	public override VaultCommand Create(IEnumerable<Xon> commnad, Flow flow)
	{
		var t = commnad.First().Name;
		var args = commnad.Skip(1).ToList();
		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		return ct.GetConstructor([typeof(Vault), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as VaultCommand;
	}

	public WalletAccount Find(AccountAddress address)
	{
		foreach(var i in Wallets)
			foreach(var j in i.Accounts)
				if(j.Address == address)
					return j;

		return null;
	}

	public Wallet FindWallet(string name)
	{
		return Wallets.Find(i => string.Compare(i.Name, name ?? Wallet.Default, true) == 0);
	}

	public Wallet CreateWallet(string name, string password, int accounts = 1)
	{
		var w = new Wallet(this, name, Enumerable.Range(0, accounts).Select(i => AccountKey.Create()).ToArray(), password);
		return w;
	}

	public Wallet CreateWallet(string name, AccountKey[] key, string password)
	{
		var w = new Wallet(this, name, key, password);
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

	public Wallet AddWallet(string name, AccountKey[] key, string password)
	{
		if(FindWallet(name) != null)
			throw new VaultException(VaultError.AlreadyExists);

		var w = CreateWallet(name, key, password);
		
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

		File.Delete(Path.Combine(Settings.Profile, name + "." + WalletExt(Cryptography)));

		Wallets.Remove(w);
	}
}
