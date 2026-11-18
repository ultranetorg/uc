using System;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Check that `bandwidth allocate` command works as expected.
/// - Basic smoke testing (default use case);
/// - Not sufficient funds for allocation attempt, fail expected;
/// Each command line parameter is input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class UserBandwidthAllocation : RdnTest
{
	Actor a;

	public UserBandwidthAllocation() : base("Bandwidth Allocate Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: false);
	}

	public void Logic()
	{
		// Not sufficient funds. This operation needs 2*12month*30days*24hours=17_280 energy. Failure expected.
		NetExec(a.Os, $"user ab bandwidth=2 months=12 by={a.User.Name} aor=expectfailure");

		TopUpUser(a.User.Name, 0, e: 17280);
		NetExec(a.Os, $"user ab bandwidth=2 months=12 by={a.User.Name}");

		TopUpUser(a.User.Name, 0, e: 100000);
		// To many month - 12 is max. Exception expected.
		ExpectException(() => NetExec(a.Os, $"user ab bandwidth=1 months=30 by={a.User.Name}"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); }

		string fmtcmd = $"user ab bandwidth={{0}} month=31 by={a.User.Name}";
		Prober.Probe(fmtcmd, f, exclude: [ProbeType.Numbers, ProbeType.NumbersBig, ProbeType.NumbersSpaced]);
		fmtcmd = $"user ab bandwidth=20 month={{0}} by={a.User.Name}";
		Prober.Probe(fmtcmd, f, exclude: [ProbeType.Numbers, ProbeType.NumbersBig, ProbeType.NumbersSpaced]);
		fmtcmd = $"user ab bandwidth=20 month=31 by={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
