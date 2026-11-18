using Uccs.Net;
using System;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;


/// <summary>
/// Author creation tests: <br/>
/// - Not sufficient funds. Fail expected. <br/>
/// - Minimal time: 1 year. <br/>
/// - Max time: 10 years. <br/>
/// - Exceeding max time of 10 years. Exception expected. <br/>
/// - With already existing title. <br/>
/// </summary>
[TestType(TestType.CommandLine)]
class AuthorCreation : FairTest
{
	Actor actor;

	public AuthorCreation() : base("Author Create Command Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		actor = CreateActor();
	}

	public void Logic()
	{
		// Not sufficient funds. Expected to fail.
		CreateAuthor(actor, "Title0", years: 1, aor: ActionOnResult.ExpectFailure);

		TopUpUser(actor.User.Name);

		// Min time: 1 year.
		CreateAuthor(actor, "Title0", years: 1);

		// Max time: 10 years.
		CreateAuthor(actor, "Title1", years: 10);

		// Exceeding max time of 10 years. Exception expected.
		ExpectException(() => CreateAuthor(actor, "Title1", years: 17));

		// Creating author with an existing title.
		// As confirmed by @Maximion - same titles are allowed, its moderator's job to resove this.
		CreateAuthor(actor, "Title0", years: 1);

		// Maximum length possible for author's title.
		string name = GetStringOfLength(Uccs.Fair.Fair.TitleLengthMaximum);
		CreateAuthor(actor, name, years: 1);

		// Exceeding maximum possible length of author's title.
		name = GetStringOfLength(1 + Uccs.Fair.Fair.TitleLengthMaximum);
		ExpectException(() =>
			CreateAuthor(actor, name, years: 1));

		ParamsTest();
	}

	void ParamsTest()
	{
		Action<string, ProbeType> a = (s, pt) => { ExpectException(() => NetExec(actor.Os, s)); };

		string fmtcmd = $"author c title={{0}} years=9 by={actor.User.Name}";
		// Title accepting crazy string with any special characters is expected behavior as confirmed
		// by @Maximion, so no probing on this.
		// Prober.Probe(fmtcmd, a);
		fmtcmd = $"author c title=\"Publisher Title\" years={{0}} by={actor.User.Name}";
		Prober.Probe(fmtcmd, a);
		fmtcmd = $"author c title=\"Publisher Title\" years=9 by={{0}}";
		Prober.Probe(fmtcmd, a);
	}
}

