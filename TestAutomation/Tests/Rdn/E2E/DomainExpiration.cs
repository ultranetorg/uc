using System;
using Uccs.Rdn;
using Uccs.Net;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This e2e test represents a usecase when user attempts to use the domain 
/// after its expiration. User acquires a domain for a year, more then a year passes, 
/// user attempts to create resource or update sub-domain policy, and is expected to fail to do so.
/// </summary>
[TestType(TestType.EndToEnd)]
class DomainExpiration : RdnTest
{
	string domain = GetUnusedLongDomainName();

	public DomainExpiration() : base("Domain Expiration E2E Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public override void Logic()
	{
		// 1. Actor creates a wallet, user.
		Actor a = CreateActor(topUp: true);

		// 2. Actor acquires domain for the duration of 1 year.
		CreateDomain(a, domain, years: 1);

		// 3. Year pases.
		TimeFastForward(TimeSpan.FromDays(365 + 1));

		// 4. Actor attempts to use expired domain: create a resource and renew it.
		// Both expected to fail.
		CreateResource(a, $"/{domain}/testResource", $"{Meaning.Raw}{{hex=123456}}", aor: ActionOnResult.ExpectFailure);
		NetExec(a.Os, $"domain renew {domain} years=3 by={a.User.Name} aor=expectfailure");
	}
}
