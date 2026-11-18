using System;

using Uccs.Nexus;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests wallet locking with `wallet lock` command. Simple smoke test; Command parameters input tested.
/// </summary>
[TestType(TestType.Generic)]
class WalletLock : RdnTest
{
	Wallet? w;

	public WalletLock() : base("Wallet Locking")
	{	}

	public override void Init()
	{
		base.Init();

		CreateMinimalNetwork();

		w = WalletCreate(Oses[1]);

		VaultExec(Oses[1], $"wallet unlock name={w.Name} password=!Market12345");
	}

	public void Logic()
	{
		VaultExec(Oses[1], $"wallet lock name={w?.Name}");

		ParamsTest();
	}

	void ParamsTest()
	{
		Action<string, ProbeType> a = (s, pt) => ExpectException(() => NetExec(Oses[1], s));

		string fmtcmd = $"wallet lock name={{0}}";
		Prober.Probe(fmtcmd, a);
	}
}
