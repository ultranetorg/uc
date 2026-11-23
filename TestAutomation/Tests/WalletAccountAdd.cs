using System;
using System.Diagnostics;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests command to add a new account to a wallet. Verifies that wallet object contains +1 account after execution.
/// </summary>
[TestType(TestType.CommandLine)]
class WalletAccountAdd : RdnTest
{
	public WalletAccountAdd() : base("Wallet Add Account Command Test") { }

	public override void Init()
	{
		base.Init();

		CreateMinimalNetwork();
	}

	public override void Logic()
	{
		int prev = Oses[1].Nexus.Vault.Wallets[0].Keys.Count;
		VaultExec(Oses[1], "wallet addaccount");
		Assert(Oses[1].Nexus.Vault.Wallets[0].Keys.Count == prev + 1);
		
	}
}
