using System;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// - Smoke test. <br/>
/// - Not sufficient funds. Fail expected. <br/>
/// - Attempt to renew for longer then 10 years in total. Fail expected. <br/>
/// - Renewal for exactly 10 years in total.<br/>
/// - Command params input-testing.
/// </summary>
[TestType(TestType.CommandLine)]
class StoreRenewal : FairTest
{
	Actor a;
	AutoId siteId;

	public StoreRenewal() : base("Site Renewal Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: false);
		TopUpUser(a.User.Name, 10 * 365 * 100); // Exact space/time required to hold a site for 3 years.
		siteId = CreateStore(a, "Site", 10, topUp: false);
		// Site renew operation spends site resources, so need to top-up the site.
		TopUpStore(siteId, 365 * 100); // Exact space/time required to hold a site for 1 year.
	}

	public override void Logic()
	{
		TimeFastForward(new(9 * 365 + 1, 0, 0, 0)); // 9 years. 

		// Smoke test.
		SiteRenew(a, 1);

		// Not sufficient funds. Fail expected.
		SiteRenew(a!, 1, ActionOnResult.ExpectFailure);

		TopUpStore(FindFairUser(a.User.Name).ModeratedStores[0], 365 * 100 * 1000);

		// Renewal is only possible for 10 years forward. Attempt to renew farther then 10 years forward. Failure expected.
		// 2 years current + 9 years renew > 10 max years).
		SiteRenew(a!, 9, ActionOnResult.ExpectFailure);

		// Maximum renewal time. (2 + 8 = 10)
		SiteRenew(a!, 8);

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[1], s)); }

		string fmtcmd = $"site r {{0}} years=8 by={this.a.User.Name}";
		Prober.Probe(fmtcmd, a);
		fmtcmd = $"site r {FindFairUser(this.a.User.Name).ModeratedStores[0]} years={{0}} by={this.a.User.Name}";
		Prober.Probe(fmtcmd, a);
		fmtcmd = $"site r {FindFairUser(this.a.User.Name).ModeratedStores[0]} years=8 by={{0}}";
		Prober.Probe(fmtcmd, a);
	}
}

