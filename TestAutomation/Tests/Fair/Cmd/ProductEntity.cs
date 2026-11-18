using System;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// - Smoke test <br/>
/// - Non-existent product id, exception expected.
/// </summary>
[TestType(TestType.CommandLine)]
class ProductEntity : FairTest
{
	Actor a;

	public ProductEntity() : base("Product Entity Command Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		CreateAuthor(a, "Author", 1, topUp: true);
		CreateProduct(a.Os, FindFairUser(a.User.Name).Authors[0], a.User.Name);
	}

	public void Logic()
	{
		AutoId productId = McvFind<Author>(FindFairUser(a.User.Name).Authors[0]).Products[0];

		// Smoke test.
		NetExec(a.Os, $"product e {productId}");

		// Non existent product id, exception expected.
		ExpectException(() => NetExec(a.Os, $"product e 1234567-0"));

		ParamsTest();
	}

	void ParamsTest()
	{
		Action<string, ProbeType> a = (s, pt) => ExpectException(() => NetExec(Oses[1], s));

		string fmtcmd = $"product e {{0}}";
		Prober.Probe(fmtcmd, a);
	}
}

