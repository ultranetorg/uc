using System.Linq;

using Uccs.Nexus;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests importing of an existing account to a wallet using an account private key with `wallet addaccount` command.
/// - Key import into a default wallet ('name' parameter is not specified).
/// - Default case. Smoke test.
/// - Importing account into a wallet that does not exist. Exception expected.
/// - Importing a bad key. Exception expected.
/// Command parameters fuzzy tested.
/// </summary>
[TestType(TestType.CommandLine)]
class WalletImportAccountByKey : RdnTest
{
	Actor a;
	Wallet newWallet;
	string key = "C9DCB9AB27B3E38D5709F0EF7F1B7B406F5DCCE7F2B7A1B4542DAE2FF6A39D90";

	public WalletImportAccountByKey() : base("Wallet Import Account By Key")
	{ }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: false);
		newWallet = WalletCreate(a.Os, "new_wallet", unlock: true);
	}

	public void Logic()
	{
		// When 'name' is not specified, account is added to the default wallet.
		VaultExec(a.Os, $"wallet addaccount key={key}");
		Assert(a.Os.Nexus.Vault.Wallets[0].Keys.Count == 1);

		// Default case. Smoke test.
		VaultExec(a.Os, $"wallet addaccount wallet={newWallet.Name} name={GetUnusedUserName()} key={key}");
		Assert(a.Os.Nexus.Vault.Wallets.First(w => w.Name == newWallet.Name).Keys.Count == 1);

		// Importing account into a wallet that does not exist. Exception expected.
		ExpectException(() => VaultExec(a.Os, $"wallet addaccount wallet=no_such_wallet key={key}"));

		// Importing a bad key. Exception expected.
		ExpectException(() => VaultExec(a.Os, $"wallet addaccount wallet={newWallet.Name} key={key[..5]}"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); };

		Prober.Probe($"wallet addaccount wallet={{0}} name={GetUnusedUserName()} key={key}", f);
		Prober.Probe($"wallet addaccount wallet={newWallet.Name} name={{0}} key={key}", f);
		Prober.Probe($"wallet addaccount wallet={newWallet.Name} name={GetUnusedUserName()} key={{0}}", f);
	}
}
