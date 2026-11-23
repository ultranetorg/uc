using System;
using System.Linq;
using System.Threading;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This test checks that 'domain entity' command works as expected.
/// - Basic smoke testing;
/// - Case with nonexistent domain, exception expected;
/// Command params input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class DomainEntity : RdnTest
{
	Actor a;
	string domain = GetUnusedLongDomainName();

	public DomainEntity() : base("Domain Entity Query Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		CreateDomain(a, domain, 1);
	}

	public override void Logic()
	{
		NetExec(a.Os, $"domain entity address={domain}");
		AssertAddressOutput(domain);

		// Case with nonexistent domain, exception expected.
		ExpectException(() => NetExec(a.Os, $"domain entity address=nosuchdomain"));

		CmdFuzzTests();
	}

	void CmdFuzzTests()
	{
		void f(string s, ProbeType pt) => ExpectException(() => NetExec(a.Os, s));

		Prober.Probe($"domain entity address={{0}}", f);
	}
}
