using System;
using System.IO;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests uos wallet file import with `wallet import` command. Simple smoke test; 
/// Command parameters input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class WalletImport : RdnTest
{
	string walletToImport;

	public WalletImport() : base("Wallet Import Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();
		
		walletToImport = Path.Combine(Oses[0].GetProfileDirPath(this), "default.uwa");
	}

	public void Logic()
	{
		VaultExec(Oses[1], $"wallet import name=imported path={walletToImport}");

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[2], s)); };

		Prober.Probe($"wallet import name={{0}} path={walletToImport}", a);
		Prober.Probe($"wallet import name={GetUnusedWalletName(Oses[2])} path={{0}}", a);
	}
}
