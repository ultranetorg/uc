using System;
using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;
using System.Diagnostics;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// </summary>
[TestType(TestType.CommandLine)]
class DomainSecurity : RdnTest
{
	Actor a1, a2;
	string domain = GetUnusedLongDomainName();

	public DomainSecurity() : base("Domain Security Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor(topUp: true);
		a2 = CreateActor(topUp: true);
	}

	public void Logic()
	{
		// TODO: What happens with the resources that live in the domain
		// when it is transfered to another owner?

		var dReg = CreateDomain(a1, domain, years: 1);

		TimeFastForward(TimeSpan.FromDays(5));

		// Actor 1 passes ownership of the domain to actor 2.
		NetExec(a1.Os, $"domain s address={domain} owner={a2.User.Name} by={a1.User.Name}");
		AssertDomainOwnership(a2.User.Id);

		TimeFastForward(TimeSpan.FromDays(5));

		// Actor 2 passes ownership of the domain back to actor 1.
		NetExec(a2.Os, $"domain s address={domain} owner={a1.User.Name} by={a2.User.Name}");
		AssertDomainOwnership(a1.User.Id);

		ParamsTest();
	}

	void AssertDomainOwnership(AutoId owner)
	{
		Domain d = FindDomain(domain);
		Assert(d.Owner == owner);
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) => ExpectException(() => NetExec(a1.Os, s));

		Prober.Probe($"domain s address={{0}} owner={a1.User.Name} by={a2.User.Name}", a);
		Prober.Probe($"domain s address={domain} owner={{0}} by={a2.User.Name}", a);
		Prober.Probe($"domain s address={domain} owner={a1.User.Name} by={{0}}", a);
	}
}