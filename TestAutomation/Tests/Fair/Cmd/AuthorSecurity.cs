using System;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// </summary>
[TestType(TestType.CommandLine)]
class AuthorSecurity : FairTest
{
	Actor a1, a2;
	AutoId authorId;

	public AuthorSecurity() : base("Author Security Command Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor(topUp: true);
		a2 = CreateActor();
		authorId = CreateAuthor(a1, "author_title", 3, topUp: true);
	}

	public void Logic()
	{
		NetExec(a1.Os, $"author security id={authorId} addowner={a2.User.Id} by={a1.User.Name}");
		Assert(McvFind<Author>(authorId).Owners.Length == 2);
		
		// Assert that new owner has an actual control of the owner, by changing author itself.
		// E.g. changing author title.
		string newTitle = "Abobius The Third";
		NetExec(a2.Os, $"author info id={authorId} title=\"{newTitle}\" by={a2.User.Name}");
		Assert(McvFind<Author>(authorId).Title == newTitle);

		// Remove original owner.
		NetExec(a2.Os, $"author security id={authorId} removeowner={a1.User.Id} by={a2.User.Name}");
		Assert(McvFind<Author>(authorId).Owners.Length == 1);

		// Assert that now removed original owner no longer has access to the author.
		newTitle = "Give my author back";
		NetExec(a1.Os, $"author info id={authorId} title=\"{newTitle}\" by={a1.User.Name} aor=expectfailure");

		ParamsTest();
	}

	void ParamsTest()
	{
		void action(string s, ProbeType pt) { ExpectException(() => NetExec(a1.Os, s)); }

		Prober.Probe($"author security id={{0}} addowner={a1.User.Id} removeowner={a1.User.Id}", action);
		Prober.Probe($"author security id={authorId} addowner={{0}} removeowner={a1.User.Id}", action);
		Prober.Probe($"author security id={authorId} addowner={a1.User.Id} removeowner={{0}}", action);
	}
}
