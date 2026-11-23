using Uccs.Net;
using Uccs.Nexus;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// - Smoke test. Basic correct user creation case.
/// - Not sufficient funds to create a user, failure expected.
/// - Top-up user and try again, expected to work. 
/// - Creation of user with already existing username and owner address. Failure expected.
/// - Creation of user with existing username but new owner address. Failure expected.
/// - Too short/ long username tests. Exceptions expected.
/// - Command-line parameters fuzz-tested.
/// </summary>
[TestType(TestType.CommandLine)]
class RdnUserCreation : RdnTest
{
	//Os os1, os2;
	(Os os, PublicKey key) u1, u2;
	string username;

	public RdnUserCreation() : base("Rdn User Creation Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		Os os1 = CreateOsNodePair();
		Wallet w = WalletCreate(os1);
		WalletUnlock(os1, w);
		username = GetUnusedUserName();
		PublicKey ak1 = WalletAddAccount(os1, w, username);
		os1.CreateSession(username);
		u1 = (os1, ak1);

		Os os2 = CreateOsNodePair();
		w = WalletCreate(os2);
		WalletUnlock(os2, w);
		PublicKey ak2 = WalletAddAccount(os2, w, username);
		os2.CreateSession(username);
		u2 = (os2, ak2);
	}

	public override void Logic()
	{
		// TODO: What if some cmd argument missing? I should think about this.

		// Smoke test. Basic correct user creation case.
		UserCreate(u1.os, username, u1.key);

		// Creation of the same user again. Failure expected.
		UserCreate(u1.os, username, u1.key, ActionOnResult.ExpectFailure);

		// Creation of an existing in rdn user. Failure expected.
		UserCreate(u2.os, username, u2.key, ActionOnResult.ExpectFailure);

		// // Username lengths tests.
		// 3 chars username, 1 char less then minimum 4 char limit. Exception expected.
		ExpectException(() => UserCreate(u1.os, "012", u1.key));
		// 33 chars username, 1 char more then maximum 32 char limit. Exception expected.
		ExpectException(() => UserCreate(u1.os, "0123456789ABCDEF0123456789ABCDEF0", u1.key));

		CmdFuzzTests();
	}

	void CmdFuzzTests()
	{
		void a(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[0], s)); }

		Prober.Probe($"user create owner={{0}} by={GetUnusedUserName()}", a);
		Prober.Probe($"user create owner={u1.key} by={{0}}", a);
	}
}

