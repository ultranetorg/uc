using System;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;


/// <summary>
/// Tests site creation functionality including specific cases: <br/>
/// - Not sufficient funds fail. <br/>
/// - Creation for minimal time - 1 year. <br/>
/// - Creation for maximal time - 10 years. <br/>
/// - Creation for a long time - 200 years. Fail expected. <br/>
/// - Creating a site with a title that already exists. This is allowed. <br/>
/// <br/>
/// Cmd arguments are individually tested for junk input. <br/>
/// <br/>
/// <b>TODO</b>: Site lifetime expiration is not implemented yet! Releases of any kind should not be 
/// allowed until then. Tests validating lifetime expiration should be implemented as soon as
/// implementation takes place.
/// </summary> 
[TestType(TestType.Generic)]
class StoreCreation : FairTest
{
	Actor a1;

	public StoreCreation() : base("Site Creation Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor();
	}

	public override void Logic()
	{
		// Not sufficient funds. Expecting fail.
		CreateStore(a1.Os, "A Nice Site", 1, a1.User.Name, aor: ActionOnResult.ExpectFailure);

		//TopUpAccount(FindFairAccount(aa1!).Id);
		TopUpUser(a1.User.Name);

		// Niminal time.
		CreateStore(a1.Os, "One Year Site", 1, a1.User.Name);

		// Max allowed time of 10 years.
		CreateStore(a1.Os, "Ten Year Site", 10, a1.User.Name);

		// Time longer that maximum 10 years. Exception expected.
		ExpectException(() => 
			CreateStore(a1.Os, "Long Site", 200, a1.User.Name));
		
		// Same title as existing site. Same titles are allowed.
		CreateStore(a1.Os, "A Nice Site", 1, a1.User.Name);

		// Wait for the 1-year site to expire.
		TimeFastForward(new(365 * 3, 0, 0)); // 3 years.
		Store s = McvFind<Store>(FindFairUser(a1.User.Name).ModeratedStores[0]);

		// Maximum possible store name length.
		string name = GetStringOfLength(Uccs.Fair.Fair.TitleLengthMaximum);
		CreateStore(a1.Os, name, 1, a1.User.Name);

		// Exceeding possible store name length. Exception expected.
		name = GetStringOfLength(Uccs.Fair.Fair.TitleLengthMaximum + 1);
		ExpectException(() => 
			CreateStore(a1.Os, name, 1, a1.User.Name, aor: ActionOnResult.ExpectFailure));

		// TODO: Site expiration is not implemented yet, that's why this does not throw exception.
		ExpectException(() => 
			NetExec(a1.Os, $"store e id={s.Id}"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType _) { ExpectException(() => NetExec(a1.Os, s)); }

		// $"site c years=3 title=\"A Nice Site\" signer={aa1} aor=retryuntilconfirmed"
		string fmtcmd = $"site c years={{0}} title=\"A Nice Site\" by={a1.User.Name}";
		Prober.Probe(fmtcmd, a);
		// Title can be anything, numbers and special characters included - hence no probe for now.
		// fmtcmd = $"site c years=3 title={{0}} signer={aa1} aor=retryuntilconfirmed";
		// Prober.Probe(fmtcmd, a);
		fmtcmd = $"site c years=3 title=\"A Nice Site\" by={{0}}";
		Prober.Probe(fmtcmd, a);
	}
}

