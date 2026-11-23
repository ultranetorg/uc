using System;

using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This tests sub-domain policy update command.
/// - Generic case, smoke testing.
/// - Change from FullOwnership to FullFreedom.
/// - Attempts to change policy of a subdomain under FullFreedom, failure expected.
/// - Attempting to update policy of a nonexistent subdomain, exception expected.
/// - Attempt to update policy of a subdomain by nonexistent username, exception expected.
/// Command parameters fuzzy tested.
/// </summary>
[TestType(TestType.CommandLine)]
class DomainPolicyUpdate : RdnTest
{
	string domain = GetUnusedLongDomainName();
	string subdomain1 = "subdomain1";
	string subdomain2 = "subdomain2";
	Actor a;

	public DomainPolicyUpdate() : base("Domain Policy Update Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		CreateDomain(a, domain, 1);
		SubDomainRegistration(a, $"{subdomain1}.{domain}", 1);
		SubDomainRegistration(a, $"{subdomain2}.{domain}", 1);
	}

	public override void Logic()
	{
		// Generic case, smoke testing.
		SubDomainPolicyUpdate(a.Os, $"{subdomain1}.{domain}", a.User.Name, OwnershipPolicy.FullOwnership);

		// Change from FullOwnership to FullFreedom.
		// After this operation "the owner of the parent domain can NOT later revoke/change ownership of the subdomain or change policy".
		SubDomainPolicyUpdate(a.Os, $"{subdomain1}.{domain}", a.User.Name, OwnershipPolicy.FullFreedom);

		// After 'FullFreedom' is applied - all attempts to change policy should fail.
		SubDomainPolicyUpdate(a.Os, $"{subdomain1}.{domain}", a.User.Name, OwnershipPolicy.FullOwnership, ActionOnResult.ExpectFailure);
		SubDomainPolicyUpdate(a.Os, $"{subdomain1}.{domain}", a.User.Name, OwnershipPolicy.FullFreedom, ActionOnResult.ExpectFailure);

		// Attempting to update policy of a nonexistent subdomain, exception expected.
		ExpectException(() => SubDomainPolicyUpdate(a.Os, $"nonexistent.{domain}", a.User.Name, OwnershipPolicy.FullFreedom));

		// Attempt to update policy of a subdomain by nonexistent username, exception expected.
		ExpectException(() => SubDomainPolicyUpdate(a.Os, $"{subdomain2}.{domain}", "nonexistent_username", OwnershipPolicy.FullFreedom, ActionOnResult.ExpectFailure));

		CmdFuzzTests();
	}

	void CmdFuzzTests()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[0], s)); };

		Prober.Probe($"domain up {{0}} by={a.User.Name} policy={OwnershipPolicy.FullOwnership} aor=retryuntilconfirmed", f);
		Prober.Probe($"domain up {subdomain1}.{domain} by={{0}} policy={OwnershipPolicy.FullOwnership} aor=retryuntilconfirmed", f);
		Prober.Probe($"domain up {subdomain1}.{domain} by={a.User.Name} policy={{0}} aor=retryuntilconfirmed", f);
		Prober.Probe($"domain up {subdomain1}.{domain} by={a.User.Name} policy={OwnershipPolicy.FullOwnership}  aor={{0}}", f);
	}
}
