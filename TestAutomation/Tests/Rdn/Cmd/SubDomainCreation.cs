using System;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests subdomain creation command `domain cs`. Basic smoke testing; Nested sub-domains; Command parameters input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class SubDomainCreation : RdnTest
{
	string domain = GetUnusedLongDomainName();
	string subdomain = "subdomain";

	User u;

	public SubDomainCreation() : base("Sub Domain Creation Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		u = CreateUserAccount(Oses[1], topUp: true);
		CreateDomain(Oses[1], domain, u.Name, 1);
	}

	public override void Logic()
	{
		NetExec(Oses[1], $"domain a address={subdomain}.{domain} for={u.Name} by={u.Name} years=1 aor=retryuntilconfirmed");
		AssertStatusConfirmedOutput();

		// Nested sub-domains
		NetExec(Oses[1], $"domain a address=subsub.{subdomain}.{domain} for={u.Name} by={u.Name} years=1 aor=retryuntilconfirmed");
		AssertStatusConfirmedOutput();
		NetExec(Oses[1], $"domain a address=subsub.subsub.{subdomain}.{domain} for={u.Name} by={u.Name} years=1 aor=retryuntilconfirmed");
		AssertStatusConfirmedOutput();

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[1], s)); };

		Prober.Probe($"domain a address={subdomain}.{domain} for={{0}} by={u.Name} years=1", a);
		Prober.Probe($"domain a address={subdomain}.{domain} for={u.Name} by={{0}} years=1", a);
		Prober.Probe($"domain a address={subdomain}.{domain} for={u.Name} by={u.Name} years={{0}}", a);
		Prober.Probe($"domain a address={subdomain}.{domain} for={u.Name} by={u.Name} years=1 aor={{0}}", a);
	}
}
