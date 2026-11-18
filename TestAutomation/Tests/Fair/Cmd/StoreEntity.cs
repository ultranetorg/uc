using System;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// - Smoke test. <br/> 
/// - Case with non-existent site id, exception expected. <br/> 
/// - Command params input-tested.
/// </summary>
[TestType(TestType.CommandLine)]
class StoreEntity : FairTest
{
	Actor a;
	public StoreEntity() : base("Site Entity Command Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		CreateStore(a, "Site", 1);
	}

	public void Logic()
	{
		//FairAccount fa = FindFairAccount(a!.aa);

		// Smoke test.
		FairUser fu = FindFairUser(a.User.Name);
		NetExec(a.Os, $"site e {fu.ModeratedStores[0]}");

		// Non-existent site id.
		ExpectException(() => NetExec(Oses[1], $"site e 1234567-0"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[1], s)); }

		string fmtcmd = $"site e {{0}}";
		Prober.Probe(fmtcmd, a);

	}
}

