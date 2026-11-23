using System;
using System.Diagnostics;
using TestAutomation.ParamProber;
using Uccs.Net;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// Check that `account entity` command works as expected. Basic smoke testing;
/// Attempt with non-existant acc address; Each command line parameter is input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class FairUserEntity : FairTest
{
	User u;
	uint userEntityCmdOutputLen = 14;

	public FairUserEntity() : base("User Entity Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		u = CreateUserAccount(Oses[1], topUp: false);
	}

	public override void Logic()
	{
		NetExec(Oses[0], $"user entity name={Net.Father0Name}");
		Assert(IsLogTailContainsSubstr(Net.Father0Signer.ToString(), userEntityCmdOutputLen));
		NetExec(Oses[1], $"user entity name={u.Name}");
		Assert(IsLogTailContainsSubstr(u.Key.ToString(), userEntityCmdOutputLen));

		// Nonexistent user name.
		ExpectException(() => NetExec(Oses[0], $"user entity name=no_such_username"));

		CmdFuzzTests();
	}

	void CmdFuzzTests()
	{
		void a(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[0], s)); };

		string fmtcmd = $"account entity {{0}}";
		Prober.Probe(fmtcmd, a);
	}
}
