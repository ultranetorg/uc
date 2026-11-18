using System;

using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// - Smoke test. <br/>
/// - Not sufficient funds attempt. Expected to fail. <br/>
/// - Renewal for more than 10 years in total. Failure expected. <br/>
/// - Renewal for maximal 10 years in total.
/// </summary>
[TestType(TestType.CommandLine)]
class AuthorRenewal : FairTest
{
	Actor a;

	public AuthorRenewal() : base("Author Renewal Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		CreateAuthor(a, "author_title", 5, topUp: false);
		TopUpAuthor(FindFairUser(a.User.Name).Authors[^1], 365 * 100);
	}

	public void Logic()
	{
		// Smoke test.
		AuthorRenew(a, 1);

		TimeFastForward(new(1, 0, 0, 0));

		// Unsufficient funds renewal attempt, expected to fail.
		AuthorRenew(a, 1, ActionOnResult.ExpectFailure);

		TopUpAuthor(FindFairUser(a.User.Name).Authors[0]);

		// Renewal for more than 10 years in total (6 + 5 > 10). Failure expected.
		AuthorRenew(a, 5, ActionOnResult.ExpectFailure);

		// Renewal for maximal 10 years in total (6 + 4 = 10).
		AuthorRenew(a, 4);

		ParamsTest();
	}

	void ParamsTest()
	{
		void action(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[1], s)); }

		Prober.Probe($"author r {{0}} years=3 by={a.User.Name}", action);
		Prober.Probe($"author r {FindFairUser(a.User.Name).Authors[^1]} years={{0}} by={a.User.Name}", action);
		Prober.Probe($"author r {FindFairUser(a.User.Name).Authors[^1]} years=3 by={{0}}", action);
	}
}
