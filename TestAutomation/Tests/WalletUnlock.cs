using Uccs.Nexus;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests `wallet unlock` command that allows to use underlying account keys.
/// - Attempt to add an account to the locked wallet. Exception expected.
/// - Attempt to unlock a wallet with wrong password. Exception expected.
/// - Default smoke test.
/// - Attempt to unlock a wallet that does not exist. Exception expected.
/// Command parameters are input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class WalletUnlock : RdnTest
{
	Wallet w;
	string pass = "my_pass";

	public WalletUnlock() : base("Wallet Unlock Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		w = WalletCreate(Oses[1], pass: pass);
	}

	public void Logic()
	{
		// Attempt to add an account to the locked wallet. Exception expected.
		ExpectException(() => WalletAddAccount(Oses[1], w, GetUnusedUserName()));

		// Attempt to unlock a wallet with wrong password. Exception expected.
		ExpectException(() => WalletUnlock(Oses[1], w, pass: "wrong_password"));

		// Smoke test.
		WalletUnlock(Oses[1], w, pass);

		// Attempt to unlock a wallet that does not exist. Exception expected.
		ExpectException(() => VaultExec(Oses[1], $"wallet unlock name=\"no_such_name\" password={pass}"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) => ExpectException(() => NetExec(Oses[1], s));

		Prober.Probe($"wallet unlock name={{0}} password={pass}", a);
		Prober.Probe($"wallet unlock name={w!.Name} password={{0}}", a);
	}
}
