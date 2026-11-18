using System;

using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// - Not sufficient funds attempt, failure expected. <br/>
/// - Smoke test. <br/>
/// - Attempt to creat a product for a foreign author (author that is not registered on a signer account address). Failure expected. <br/>
/// </summary>
[TestType(TestType.CommandLine)]
class ProductCreation : FairTest
{
	Actor a1, a2;
	AutoId a1AuthId;

	public ProductCreation() : base("Product Creation Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor(topUp: true);
		CreateAuthor(a1, "title", 9);
		a1AuthId = FindFairUser(a1.User.Name).Authors[^1];

		a2 = CreateActor(topUp: true);
		CreateAuthor(a2, "title", 1);
	}

	public void Logic()
	{
		// Not sufficient funds attempt. Failure expected.
		CreateProduct(a1.Os, a1AuthId, a1.User.Name, aor: ActionOnResult.ExpectFailure);

		TopUpAuthor(a1AuthId);

		// Smoke test.
		CreateProduct(a1.Os, a1AuthId, a1.User.Name);

		// Creating a product for a foreign author. Failure expected.
		CreateProduct(a2.Os, a1AuthId, a2.User.Name, aor: ActionOnResult.ExpectFailure);

		ParamsTest();
	}

	void ParamsTest()
	{
		void action(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[1], s)); }

		Prober.Probe($"product c {{0}} by={a1.User.Name}", action);
		Prober.Probe($"product c {a1AuthId} by={{0}}", action);
	}
}

