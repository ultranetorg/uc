using Uccs.Net;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This e2e tests represents a use case when user B attempts to acquire domain that 
/// is already acquired by user A. User B is expected to get failedornotfound status on his request.
/// </summary>
[TestType(TestType.EndToEnd)]
class DomainAcquisitionConflict : RdnTest
{
	string domain = GetUnusedLongDomainName();

	public DomainAcquisitionConflict() 
		: base("Domain Acquisition Conflict E2E Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public override void Logic()
	{
		// 1. Users A and B create wallets, users.
		Actor a = CreateActor(topUp: true);
		Actor b = CreateActor(topUp: true);
		// 2. User A acquires domain.
		CreateDomain(a, domain, years: 1);

		// 3. User B attempts to acquire domain already acquired by user A earlier and fails.
		CreateDomain(b, domain, years: 1, ActionOnResult.ExpectFailure);
	}
}
