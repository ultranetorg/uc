using Uccs.Net;

using TestAutomation.ParamProber;
using System.Diagnostics;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Check that `user entity` command works.
/// - Basic smoke testing, asserted that console output contains owner's account address;
/// - Attempt with nonexistent user name, exception expected;
/// Each command line parameter is fuzzy tested.
/// </summary>
[TestType(TestType.CommandLine)]
class RdnUserEntity : RdnTest
{
	User u;
	uint userEntityCmdOutputLen = 14;

	public RdnUserEntity() : base("User Entity Test") {}

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
