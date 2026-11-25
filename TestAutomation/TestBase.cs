using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using CommonUtils;
using JunitReporter;
using O;

using Uccs;
using Uccs.Fair;
using Uccs.Net;
using Uccs.Nexus;
using Uccs.Nexus.CLI;
using Uccs.Rdn;
using Uccs.Rdn.CLI;


namespace TestAutomation;

abstract public class TestBase
{
	// From "Simulation":
	public Flow Flow;
	//public string WorkPath;


	public required string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;


	public OsGroup Group;
	public McvNet Net;
	public List<Os> Oses => Group.Oses;



	protected readonly SimulationClock Clock = new();
	protected abstract McvNode GetNode(Os os);
	protected abstract McvNodeSettings CreateSettings(Os os);
	protected abstract McvCli GetCli(Os os);

	public static string UO_TA_LOGECHO;


	public TestBase(string testName)
	{
		if(string.IsNullOrEmpty(testName))
			throw new Exception("Test name is mandatory. Pass it to the base constructor.");

		Name = testName;

		NodeGlobals.NoWait							= true;
		NodeGlobals.InfiniteTimeouts				= true;
		NodeGlobals.TimeoutAfterTransactingError	= 1;
		NodeGlobals.CatchCorrupted					= false;
		NodeGlobals.ForceApproveOutworlds			= true;

		Flow = new Flow("Simulation", new Log());

		UO_TA_LOGECHO = Environment.GetEnvironmentVariable("UO_TA_LOGECHO");

		if(UO_TA_LOGECHO == "all")
		{
			Flow.Log.Reported += (logMsg) =>
			{
				foreach(string s in logMsg.Text)
					Reporter.Message(StandardOutputStreamType.Out, s);
			};
		}
	}

	/// <summary> Test preparation step: set up the network, actors and data. Runs before <see cref="Logic"/>. </summary>
	public virtual void Init() { }

	/// <summary> The test itself. Throw to fail. </summary>
	public abstract void Logic();

	protected static string Aor(ActionOnResult aor) => $"aor={aor.ToString().ToLower()}";

	public virtual List<Os> CreateMinimalNetwork()
	{
		List<Os> oses = [];

		oses.Add(Group.CreateOs("U0", "f=2 nn", Net.Father0EP.IP));
		CreateNode(oses[^1], "mcv chain g=2 s run");
		oses.Add(Group.CreateOs("U1", "f=2 nn", Group.Initials[1]));
		CreateNode(oses[^1], "mcv chain g=2 s run");
		oses.Add(Group.CreateOs("U2", "f=2 nn", Group.Initials[2]));
		CreateNode(oses[^1], "mcv chain g=2 s run");

		SyncAllPeering();

		return oses;
	}

	protected virtual void Configure(Os os, Xon args, McvNodeSettings settings) { }
	public abstract McvNode StartNode(Os os, McvNodeSettings settings);
	public abstract Os CreateOsNodePair();

	public Node CreateNode(Os os, string args, Action<McvNodeSettings> configure = null)
	{
		Flow.ThrowIfAborted();

		McvNode node = null;
		var x = new Xon(args);

		var settings = CreateSettings(os);

		settings.Api							= new() { LocalIP = os.IP };
		settings.Peering.Endpoint				= new(os.IP, Net.PpiPort);
		settings.Peering.PermanentMin			= 2;
		settings.Peering.InitialRandomization	= false;
		settings.PpcTimeout						= int.MaxValue;
		settings.TransactingTimeout				= int.MaxValue;

		if(x.Has("mcv"))
		{
			settings.Mcv = new();

			if(x.Has("chain"))
				settings.Mcv.Chain = new();
		}

		if(x.Has("g"))
			settings.Mcv.Memberships = os.Nexus.Vault.Wallets[0].Keys.Select(i => new MembershipSettings { Generator = i.Alias, Beneficiary = i.Alias }).Take(x.Get<int>("g")).ToList();

		configure?.Invoke(settings);

		Configure(os, x, settings);

		settings.Save();

		if(x.Has("run"))
		{
			node = StartNode(os, settings);

			// Register the node right away so that Abort() stops it even if we are cancelled below.
			os.Nodes.Add(node);

			if(x.Has("s"))
				foreach(var i in settings.Mcv.Memberships)
					lock(node.Peering.Lock)
						node.Peering.CreateSession(os.Name, i.Generator, null);
		}

		os.NodeStarted?.Invoke(node);

		return node;
	}

	protected void SyncAllPeering()
	{
		while(McvPeering.All.Any(i => !i.MinimalPeersReached) && !McvPeering.All.Where(i => i.Mcv != null).Any(i => i.Synchronization == Synchronization.Synchronized))
			Flow.Wait(100);
	}

	protected object VaultExec(Os os, string cmd)
	{
		Flow.Log?.Report(cmd);

		var args = new Xon(cmd);
		args.Add(Apc.CredentialsKeyword).Value = OsGroup.AdminPassword;

		return new NexusCli(os.Nexus) { Application = os.Name, NexusSettings = os.Nexus.Settings, VaultSettings = os.Nexus.Vault.Settings }.Execute(args.Nodes, Flow.CreateNested(cmd));
	}

	protected object NexusExec(Os os, string cmd)
	{
		Flow.Log.Report(this, cmd);

		var args = new Xon(cmd).Nodes;

		return new NexusCli(os.Nexus) { Application = os.Name }.Execute(args, Flow.CreateNested(cmd));
	}

	public virtual object NetExec(Os os, string cmd)
	{
		Flow flow = Flow.CreateNested("CLI");
		flow.Log.Report(this, cmd);

		var p = GetCli(os);

		return p.Execute(new Xon(cmd).Nodes, flow);
	}

	protected object NetExec(Os os, IEnumerable<Operation> operations, string username, bool expectFailure = false)
	{
		TransactionApe transact()
		{
			return new TransactApc
			{
				Operations = operations,
				User = username,
				ActionOnResult = expectFailure ? ActionOnResult.ExpectFailure : ActionOnResult.RetryUntilConfirmed
			}
			.Execute(GetNode(os), null, null, Flow) as TransactionApe;
		}

		var t = transact();

		do
		{
			t = new OutgoingTransactionApc { Tag = t.Tag }.Execute(GetNode(os), null, null, Flow) as TransactionApe;

			if(t.Status == TransactionStatus.FailedOrNotFound && t.__ExpectedStatus == ActionOnResult.CancelOnFailure)
			{
				t = transact();
			}

			Flow.Wait(100);
		}
		while(t.Status != (expectFailure ? TransactionStatus.FailedOrNotFound : TransactionStatus.Confirmed));

		while(Group.Oses.SelectMany(i => i.Nodes).Where(i => i.Net == Net && i.Mcv != null && i.Peering.Synchronization == Synchronization.Synchronized).DistinctBy(i => i.Mcv.LastConfirmedRound.Id).Count() != 1)
			Flow.Wait(10);

		return t;
	}

	public User FindUser(string name)
	{
		var m = Oses[0].FindNode<McvNode>().Mcv;

		lock(m.Lock)
		{
			return m.Users.Latest(name);
		}
	}

	protected static void Assert(bool condition, string failMessage = "Assertion failed.")
	{
		if(condition) return;
		throw new Exception(failMessage);
	}

	/// <summary> In RDN user creation is free: user creates itself. </summary>
	public User UserCreate(Os creatorOs, string username, PublicKey owner, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(creatorOs, $"user create owner={owner} by={username} {Aor(aor)}");
		return FindUser(username);
	}

	protected DomainNameAcquisition AcquireDomainName(Os os, string name, string username, uint years, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return NetExec(os, $"domainname a name={name} years={years} by={username} {Aor(aor)}") as DomainNameAcquisition;
	}

	protected DomainNameAcquisition AcquireDomainName(Actor a, string name, uint years, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return NetExec(a.Os, $"domainname a name={name} years={years} by={a.User.Name} {Aor(aor)}") as DomainNameAcquisition;
	}

	public DomainCreation CreateDomain(Os os, string address, string username, uint years, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return NetExec(os, $"domain c name={address} years={years} by={username} {Aor(aor)}") as DomainCreation;
	}

	public DomainCreation CreateDomain(Actor a, string address, uint years, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return CreateDomain(a.Os, address, a.User.Name, years, aor);
	}

	public void AcquireDomainWithDomainName(Actor a, string domainName, uint years)
	{
		AcquireDomainName(a, domainName, years);
		CreateDomain(a, domainName, years);
	}

	public DomainCreation SubDomainRegistration(Os os, string address, string newSubdomainOwnerUsername, string domainOwnerUsername, uint years, OwnershipPolicy policy = OwnershipPolicy.FullOwnership, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return NetExec(os, $"domain a address={address} years={years} policy={policy} for={newSubdomainOwnerUsername} by={domainOwnerUsername} {Aor(aor)}") as DomainCreation;
	}

	public DomainCreation SubDomainRegistration(Actor a, string address, uint years, OwnershipPolicy policy = OwnershipPolicy.FullOwnership, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return SubDomainRegistration(a.Os, address, a.User.Name, a.User.Name, years, policy, aor);
	}

	public void SubDomainPolicyUpdate(Os os, string address, string username, OwnershipPolicy newPolicy, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(os, $"domain up address={address} by={username} policy={newPolicy} {Aor(aor)}");
	}

	public ResourceCreation CreateResource(Os os, string address, string data, string username, bool dependable = false, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		string cmd = $"resource create address={address} ";
		if(!string.IsNullOrEmpty(data)) cmd += $"data={data} ";
		if(dependable) cmd += "dependable ";
		cmd += $"by={username} {Aor(aor)}";
		return NetExec(os, cmd) as ResourceCreation;
	}

	public ResourceCreation CreateResource(Actor a, string address, string data = "", bool dependable = false, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return CreateResource(a.Os, address, data, a.User.Name, dependable, aor);
	}

	public ResourceCreation CreateResourceRaw(Actor a, string address, string hexData, bool dependable = false, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return CreateResource(a.Os, address, $"{Meaning.Raw}{{hex={hexData}}}", a.User.Name, dependable, aor);
	}

	public ResourceCreation CreateResourceFile(Actor a, string address, Urn urn, ContentType ctype = ContentType.File, bool dependable = false, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return CreateResource(a.Os, address, $"{Meaning.ExternalContent}/{ctype}{{address={urn}}}", a.User.Name, dependable, aor);
	}

	public LocalReleaseApe CreateRelease(Os os, string source)
	{
		return NetExec(os, $"release create source={source}") as LocalReleaseApe;
	}

	protected PackageApe CreatePackage(Actor a, string address, string source, bool publish = true, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return (PackageApe)NexusExec(a.Os, $"package c address={address} source={source} {(publish ? "publish" : "")} {Aor(aor)} by={a.User.Name}");
	}

	/// <summary> Get user's spacetime and energy balance.</summary>
	/// <returns> Tuple(spacetime(byte/days), energy)</returns>
	public (long, long) GetBalance(string userName)
	{
		User u = NetExec(Oses[0], $"user entity name={userName}") as User;
		return (u.Spacetime, u.Energy);
	}

	public DateTime GetDomainExpirationDate(string domainAddress)
	{
		NetExec(Oses[0], $"domain entity {domainAddress}");
		string expirationStr = LogContains("Expiration :")!;
		uint exp = uint.Parse(expirationStr.Split().Last());

		DateTime expirationDate = Time.Start + new TimeSpan((int)exp, 0, 0, 0);
		return expirationDate;
	}

	public int GetDomainTimeLeftDays(string domainAddress)
	{
		DateTime domainExp = GetDomainExpirationDate(domainAddress);
		DateTime now = new DateTime(Clock.Now.Year, Clock.Now.Month, Clock.Now.Day); // discard day time(hours, seconds, etc.), only keep date
		TimeSpan left = domainExp - now;
		return (int)left.TotalDays;
	}

	/// <summary> User to user </summary>
	public void UtilityTransactionU2U(Os os, AutoId from, string byUsername, AutoId to, long st = 0, long e = 0) {
		NetExec(os, $"utility t from=User/{from} to=User/{to} st={st} e={e} by={byUsername}");
	}

	public void UtilityTransactionU2U(Actor fromActor, Actor toActor, long st = 0, long e = 0)
	{
		NetExec(fromActor.Os, $"utility t from=User/{fromActor.User.Id} to=User/{toActor.User.Id} st={st} e={e} by={fromActor.User.Name}");
	}

	public User TopUpUser(string username, uint st = 10000000, uint e = 100000)
	{
		User u = FindUser(username);
		//NetExec(Oses[0], $"utility t from=User/{Net.Father0Id} to=User/{u.Id} st={st} e={e} by={Net.Father0Name}");
		UtilityTransactionU2U(Oses[0], Net.Father0Id, Net.Father0Name, u.Id, st, e);
		return FindUser(username);
	}

	public Wallet WalletCreate(Os os, string name = null, string pass = "!Market12345", bool unlock = false)
	{
		Wallet w = (Wallet)VaultExec(os, $"wallet create name={(string.IsNullOrEmpty(name) ? os.Nexus.Vault.Wallets.Count : name)} accounts=0 password={pass}");
		if(unlock) WalletUnlock(os, w, pass);
		return w;
	}

	public void WalletUnlock(Os os, Wallet w, string pass = "!Market12345")
	{
		VaultExec(os, $"wallet unlock name={w.Name} password={pass}");
	}

	public PublicKey WalletAddAccount(Os os, Wallet targetWallet, string newUsername)
	{
		return (PublicKey)VaultExec(os, $"wallet ak wallet={targetWallet.Name} alias={newUsername}");
	}

	public void CreateLink(Os os,
						string from,
						string to,
						string username,
						ResourceLinkType type = ResourceLinkType.None,
						ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(os, $"link create from={from} to={to} type={type} by={username} {Aor(aor)}");
	}

	/// <summary> Sugar wrapper to use with 'class Actor'</summary>
	public void CreateLink(Actor a,
						string from,
						string to,
						ResourceLinkType type = ResourceLinkType.None,
						ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		CreateLink(a.Os, from, to, a.User.Name, type, aor);
	}

	/// <summary> Sugar wrapper to use with Ura's </summary>
	public void CreateLink(Actor a,
						Ura from,
						Ura to,
						ResourceLinkType type = ResourceLinkType.None,
						ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		CreateLink(a.Os, from.ToString(), to.ToString(), a.User.Name, type, aor);
	}

	/// <summary>
	/// This method creates a wallet with default password !Market12345, creates a user account in it, creates this user in the distributed database,
	/// and then optionally tops up this user. Top-up funds are taken from Father0.
	/// </summary>
	/// <param name="os">Uos where new wallet with a new account lives</param>
	/// <param name="fatherUos">Uos of Father0</param>
	/// <param name="topUp">Should the new account be topped up?</param>
	/// @Todo This is not a good name for this method.
	public User CreateUserAccount(Os os, bool topUp)
	{
		Wallet w = WalletCreate(os);
		WalletUnlock(os, w);

		string username = GetUnusedUserName();
		PublicKey key = WalletAddAccount(os, w, username);
		
		os.CreateSession(username);

		User u = UserCreate(os, username, key);

		if(topUp) u = TopUpUser(u.Name);

		return u;
	}


	public Actor CreateActor(Os os = null, bool topUp = false)
	{
		os ??= CreateOsNodePair();
		User u = CreateUserAccount(os, topUp);
		return new(os, u);
	}

	public IPAddress GetUnusedIp()
	{
		foreach(var i in Group.Initials)
		{
			bool taken = Oses.Any((os) => os.IP.Equals(i));
			if(!taken) return i;
		}

		throw new Exception("Can't get unused IP: all IPs taken. Maybe allocate more OsLevel.Initials?");
	}

	static uint domainGenIdx = 0;
	public static string GetUnusedLongDomainName()
	{
		domainGenIdx += 1;
		return "qwertyuiopasdf" + domainGenIdx;
	}

	public bool IsLogTailContainsSubstr(string substr, uint tailLen)
	{
		var tail = Flow.Log.Messages.ToArray()[^((int)tailLen + 1)..];
		foreach(var m in tail)
		{
			foreach(string t in m.Text)
				if(t.Contains(substr)) return true;
		}
		return false;
	}

	public string LogContains(string s)
	{
		for(int i = Flow.Log.Messages.Count - 1; i >= 0; --i)
		{
			LogMessage m = Flow.Log.Messages.ToArray()[i];
			foreach(string str in m.Text)
				if(str.Contains(s)) return str;
		}

		return null;
	}

	public string GetLogLineWithSubstr(string substr)
	{
		foreach(LogMessage m in Flow.Log.Messages)
		{
			foreach(string str in m.Text)
				if(str.Contains(substr)) return str;
		}

		throw new Exception($"Log record with substring \"{substr}\" could not be found. Use \"LogContains()\" to ensure substring is present first.");
	}

	protected string GetStringOfLength(uint len)
	{
		StringBuilder b = new();
		for(int i = 0; i < len; ++i)
			b.Append("a");
		return b.ToString();
	}

	public string GetHugeLoremIpsum(uint kilobytes, bool specialSymbols = false, bool spaces = true)
	{
		var strb = new StringBuilder();
		string str_1kb = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer commodo libero nec pulvinar mollis. Etiam tempor purus nunc, et ornare neque iaculis id. Proin commodo quis mi id vulputate. Nam enim lacus, viverra sit amet ex vitae, auctor tincidunt purus. Praesent velit velit, ultrices in tellus quis, interdum auctor quam. Sed volutpat gravida nisl in sodales. Maecenas aliquam diam tellus, a iaculis sapien tincidunt mollis. Etiam libero ante, accumsan at vulputate at, interdum vitae mi. Vestibulum rutrum v";
		if(!specialSymbols) str_1kb = str_1kb.Replace('.', ' ').Replace(',', ' ');
		if(!spaces) str_1kb = str_1kb.Replace(' ', '_');

		string r = string.Empty;
		for(int i = 0; i < kilobytes; ++i)
		{
			strb.Append(str_1kb);
		}
		return strb.ToString();
	}

	public string GetHexString(int bytes)
	{
		Assert(bytes > 0);
		StringBuilder builder = new();
		byte x = 0;
		for(int i = 0; i < bytes; ++i)
		{
			x++;
			if(x > 255) x = 0;
			builder.Append(Convert.ToHexString([x]));
		}
		return builder.ToString();
	}

	public string GetTestImagePath()
	{
		//string projectFolder = Directory.GetCurrentDirectory();
		string imagePath = Path.Join(Directory.GetCurrentDirectory(), "TestData", "test_image.jpg");
		Assert(System.IO.File.Exists(imagePath), "TestAutomation TestData folder must contain \"test_image.jpg\" test image.");
		return imagePath;
	}

	/// <summary>
	/// Creates a folder with specified name in the TestData directory and populates it
	/// with 5 text files with some text.
	/// </summary>
	/// <param name="dirName">Name of the new directory</param>
	/// <returns>Full path to the new directory</returns>
	public string CreateMinimalReleaseDirectory(string dirName)
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", dirName);
		Directory.CreateDirectory(path);
		System.IO.File.WriteAllText(Path.Combine(path, "File1.txt"), "File1 content.");
		System.IO.File.WriteAllText(Path.Combine(path, "File2.txt"), "File2 content.");
		System.IO.File.WriteAllText(Path.Combine(path, "File3.txt"), "File3 content.");
		System.IO.File.WriteAllText(Path.Combine(path, "File4.txt"), "File4 content.");
		System.IO.File.WriteAllText(Path.Combine(path, "File5.txt"), "File5 content.");
		return path;
	}

	/// <summary>
	/// Creates a file with specified name in the TestData directory and populates it
	/// with some text.
	/// </summary>
	/// <param name="fileName">Name of the new file.</param>
	/// <param name="big"> When `true` - writes 100mb of content into the file. </param>
	/// <returns>Full path to the new file.</returns>
	public string CreateTestFile(string fileName, bool big = false)
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);
		if(!big) System.IO.File.WriteAllText(path, "Relese file content.");
		else System.IO.File.WriteAllText(path, GetHugeLoremIpsum(1024 * 100, true, true));
		
		return path;
	}

	public void AssertStatusConfirmedOutput()
	{
		string statusStr = LogContains("Status : ") ?? throw new Exception("Output contains no \"Status\" entry.");
		if(!statusStr.Contains("Confirmed")) throw new Exception("Status is not \"Confirmed\".");
	}

	public void AssertAddressOutput(string expected)
	{
		string addressStr = LogContains("Address : ") ?? throw new Exception("Output contains no \"Address\" entry.");
		if(!addressStr.Contains(expected)) throw new Exception($"Address value do not match with the expected value: \"{expected}\".");
	}

	public void AssertSignerOutput(string expected)
	{
		string signerStr = LogContains("Signer : ") ?? throw new Exception("Output contains no \"Signer\" entry.");
		if(!signerStr.Contains(expected)) throw new Exception($"Ouput contains wrong signer. Expected: {expected}");
	}

	public void ExpectException(Action a)
	{
		try
		{
			a.Invoke();
			throw new ShouldFailException("This should throw exception.");
		}
		catch(Exception e)
		{
			if(e is ShouldFailException) throw e;
		}
	}

	public void TimeFastForward(TimeSpan time)
	{
		Clock.Add(time);
	}

	public virtual void Abort()
	{
		Flow.Abort();

		List<Exception> errors = [];

		// Stop every Os even if some fail, so that nothing is left running.
		foreach(Os os in Oses.ToList())
		{
			try
			{
				os.Stop();
			}
			catch(Exception e)
			{
				errors.Add(e);
			}

			Oses.Remove(os);
		}

		Flow.Log.Messages.Clear();

		if(errors.Count > 0)
			throw new AggregateException("Failed to abort the test.", errors);
	}

	// This variable is used to generate account address hex strings.
	static uint accGenIdx = 0;
	// It is not actually unused, it's just 99% unused.
	public PublicKey GetUnusedAccountAddress()
	{
		PublicKey aa = PublicKey.Parse($"0x{accGenIdx:X40}");
		accGenIdx += 1;
		return aa;
	}

	protected string GetUnusedUserName()
	{
		int i = 0;
		while (true) {
			string name = $"user{i}";
			if(FindUser(name) == null)
			{
				return name;
			}
			i += 1;
		}
	}

	public string GetUnusedWalletName(Os os)
	{
		int i = 0;
		while(true)
		{
			string maybe = "wallet" + i;
			if(os.Nexus.Vault.Wallets.Find((w) => w.Name == maybe) == null) return maybe;
			i++;
		}
	}

	/// <summary>
	/// Recursively compares file/directory count and names, and file contents in a text form.
	/// </summary>
	/// <param name="ignoreHidden"> Ignore hidden files (filenames starting with '.') </param>
	protected bool AreDirectoryContentsSimilar(string dir1, string dir2, bool ignoreHidden = false)
	{
		string[] dir1Fs = Directory.GetFiles(dir1);
		string[] dir2Fs = Directory.GetFiles(dir2);
		if(ignoreHidden)
		{
			dir1Fs = dir1Fs.Where((f) => !Path.GetFileName(f).StartsWith('.')).ToArray();
			dir2Fs = dir2Fs.Where((f) => !Path.GetFileName(f).StartsWith('.')).ToArray();
		}
		if(dir1Fs.Length != dir2Fs.Length) return false;
		for(int i = 0; i < dir1Fs.Length; ++i)
		{
			if(Path.GetFileName(dir1Fs[i]) != Path.GetFileName(dir2Fs[i]))
				return false;
			if(System.IO.File.ReadAllText(dir1Fs[i]) != System.IO.File.ReadAllText(dir2Fs[i]))
				return false;
		}
		string[] dir1Dirs = Directory.GetDirectories(dir1);
		string[] dir2Dirs = Directory.GetDirectories(dir2);
		if(dir1Dirs.Length != dir2Dirs.Length) return false;
		for(int i = 0; i < dir1Dirs.Length; ++i)
		{
			if(Path.GetDirectoryName(dir1Dirs[i]) != Path.GetDirectoryName(dir2Dirs[i]))
				return false;
			if(!AreDirectoryContentsSimilar(dir1Dirs[i], dir2Dirs[i], ignoreHidden))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Calculates how much spacetime is needed to host the file for number of years.
	/// In byte/days.
	/// </summary>
	public static long GetFileSpacetimeRequirement(string filePath, int days = 365)
	{
		// +100 byte/days - baseline cost of any entity.
		return (new FileInfo(filePath).Length + 100) * days;
	}

	/// <summary> Due to specifics of MCV protocol - for new nodes to get synchronized, new blocks should be created,
	/// just enough for a new node to confirm the next round by itself.
	/// Use this method after creation of new UOSs to make them operational. </summary>
	//public void TransactUntilSync()
	//{
	//	while(true)
	//	{
	//		bool sync = !Oses.Any((os) => os.Nodes.Any((n) => n.Peering.Synchronization != Synchronization.Synchronized));
	//		if(sync) return;
	//		Thread.Sleep(200);
	//		sync = !Oses.Any((os) => os.Nodes.Any((n) => n.Peering.Synchronization != Synchronization.Synchronized));
	//		if(!sync)
	//		{
	//			UserCreate(GetUnusedUserName(), Net.Father0Signer);
	//		}
	//		Thread.Sleep(200);
	//	}
	//}

	/// <summary> Get account spacetime and energy difference after and before the function execution.</summary>
	/// <returns> Tuple(spacetime(byte/days), energy)</returns>
	public (long stDiff, long eDiff) GetUserActionTokenDiff(string userName, Action a)
	{
		(long st, long e) = GetBalance(userName);
		a();
		(long st_after, long e_after) = GetBalance(userName);
		return (st_after - st, e_after - e);
	}

	public T McvFind<T>(AutoId id) where T : class
	{
		McvNode node = GetNode(Oses[0]);
		T t = null;

		while(true)
		{
			bool synced = node.Peering.Synchronization == Synchronization.Synchronized;

			Mcv mcv = node.Mcv;

			lock(mcv.Lock)
			{
				if(mcv is FairMcv fairMcv)
				{
					if(typeof(T) == typeof(Store)) t = fairMcv.Stores.Latest(id) as T;
					if(typeof(T) == typeof(Author)) t = fairMcv.Authors.Latest(id) as T;
					if(typeof(T) == typeof(Uccs.Fair.Product)) t = fairMcv.Products.Latest(id) as T;
					if(typeof(T) == typeof(Publication)) t = fairMcv.Publications.Latest(id) as T;
					if(typeof(T) == typeof(Category)) t = fairMcv.Categories.Latest(id) as T;
					if(typeof(T) == typeof(Proposal)) t = fairMcv.Proposals.Latest(id) as T;
					if(typeof(T) == typeof(ProposalComment)) t = fairMcv.ProposalComments.Latest(id) as T;
					if(typeof(T) == typeof(Review)) t = fairMcv.Reviews.Latest(id) as T;
				}
				if(mcv is RdnMcv rdnMcv)
				{
					if(typeof(T) == typeof(Resource)) t = rdnMcv.Resources.Latest(id) as T;
				}
			}

			if(t == null || !synced) Flow.Wait(100);
			else break;
		}
		return t;
	}
}

public static class Extensions
{
	public static void WaitSync(this Os os, Flow flow)
	{
		while(true)
		{
			bool synced = os.FindNode<FairNode>().Peering.Synchronization == Synchronization.Synchronized;
			if(synced) return;
			flow.Wait(100);
		}
	}

	public static T McvFind<T>(this Os os, AutoId id, Flow flow) where T : class
	{
		McvNode node = os.Nodes[0];
		T? t = null;

		while(true)
		{
			bool synced = node.Peering.Synchronization == Synchronization.Synchronized;

			Mcv mcv = node.Mcv;
			lock(mcv.Lock)
			{
				if(mcv is FairMcv fairMcv)
				{
					if(typeof(T) == typeof(Store)) t = fairMcv.Stores.Latest(id) as T;
					if(typeof(T) == typeof(Author)) t = fairMcv.Authors.Latest(id) as T;
					if(typeof(T) == typeof(Uccs.Fair.Product)) t = fairMcv.Products.Latest(id) as T;
					if(typeof(T) == typeof(Publication)) t = fairMcv.Publications.Latest(id) as T;
					if(typeof(T) == typeof(Category)) t = fairMcv.Categories.Latest(id) as T;
					if(typeof(T) == typeof(Proposal)) t = fairMcv.Proposals.Latest(id) as T;
					if(typeof(T) == typeof(ProposalComment)) t = fairMcv.ProposalComments.Latest(id) as T;
				}
			}
			if(t == null || !synced) flow.Wait(100);
			else break;
		}
		return t;
	}

}
