using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests `domain renew` command - prolongation of a domain name rent. Basic smoke testing;
/// Command parameter input testing.
/// </summary>
[TestType(TestType.CommandLine)]
class DomainRenew : RdnTest
{
	string domain = GetUnusedLongDomainName();
	Actor a;

	public DomainRenew() : base("Domain Renew Test")	{}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		CreateDomain(a, domain, 1);
	}

	public void Logic()
	{
		TimeFastForward(new(1, 0, 0, 0)); // 1 day.
		NetExec(a.Os, $"domain renew name={domain} years=3 by={a.User.Name}");

		// Attempt to renew nonexistent domain. Exception expected.
		ExpectException(() => NetExec(a.Os, $"domain renew name={GetUnusedLongDomainName()} years=3 by={a.User.Name}"));

		// Attempt to Renew for too long (> 10 years in total). Failure expected.
		TimeFastForward(new(1, 0, 0, 0)); // 1 day.
		NetExec(a.Os, $"domain renew name={domain} years=8 by={a.User.Name} aor=expectfailure");

		CmdFuzzTests();
	}

	void CmdFuzzTests()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[1], s)); };

		Prober.Probe($"domain renew {{0}} years=1 by={a.User.Name} aor=retryuntilconfirmed", f);
		Prober.Probe($"domain renew {domain} years={{0}} by={a.User.Name} aor=retryuntilconfirmed", f);
		Prober.Probe($"domain renew {domain} years=1 by={{0}} aor=retryuntilconfirmed", f);
		Prober.Probe($"domain renew {domain} years=1 by={a.User.Name} aor={{0}}", f);
	}
}
