using System;
using System.Diagnostics;
using System.IO;
using O;
using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests wallet creation process via `wallet create` command . 
/// Smoke test + wallet file creation validation + console output validation; 
/// Cmd fuzzy test.
/// </summary>
[TestType(TestType.Generic)]
class WalletCreation : RdnTest
{
	public WalletCreation() : base("Wallet Creation") { }

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public void Logic()
	{
		// TODO: Test "accounts" parameter. (Why does it even exists? ?)

		string dir = Oses[1].GetProfileDirPath(this);

		int countPreFiles = Directory.GetFiles(dir).Length;
		int countPreWallets = Oses[1].Nexus.Vault.Wallets.Count;

		CreateWalletValidatingConsoleOutput(Oses[1]);

		Assert(Directory.GetFiles(dir).Length == countPreFiles + 1);
		Assert(Oses[1].Nexus.Vault.Wallets.Count == countPreWallets + 1);

		countPreFiles = Directory.GetFiles(dir).Length;
		countPreWallets = Oses[1].Nexus.Vault.Wallets.Count;

		CreateWalletValidatingConsoleOutput(Oses[1]);

		Assert(Directory.GetFiles(dir).Length == countPreFiles + 1);
		Assert(Oses[1].Nexus.Vault.Wallets.Count == countPreWallets + 1);

		ParamsTest();
	}

	void CreateWalletValidatingConsoleOutput(Os os)
	{
		Flow.Log.Messages.Clear();
		WalletCreate(os);
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) => ExpectException(() => VaultExec(Oses[1], s));

		Prober.Probe($"wallet create name={{0}} password=!Market12345", a, 
			only: [ProbeType.LatinSpecial, ProbeType.NumbersSpecial]);
	}
}
