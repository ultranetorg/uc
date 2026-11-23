using System;
using System.Collections.Immutable;

using Uccs.Net;

using TestAutomation.ParamProber;
using System.Diagnostics;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This verifies process of listing user's authors.
/// Authors created for a user, and the 'user la' output entry count is verified.
/// </summary>
[TestType(TestType.CommandLine)]
class AccountListAuthors : FairTest
{
	Actor a;
	public AccountListAuthors() : base("Account List Authors Command Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
	}

	public override void Logic()
	{
		VerifyAuthorsListOutput();

		// Add 3 authors.
		CreateAuthor(a, "AuthorTitle", 1);
		CreateAuthor(a, "AuthorTitle", 1);
		CreateAuthor(a, "AuthorTitle", 1);

		VerifyAuthorsListOutput();

		// Add some more authors.
		CreateAuthor(a, "AuthorTitle", 1);
		CreateAuthor(a, "AuthorTitle", 1);
		CreateAuthor(a, "AuthorTitle", 1);
		CreateAuthor(a, "AuthorTitle", 1);

		VerifyAuthorsListOutput();

		ParamsTest();
	}

	// Verify that id's in output match with the ones in the entity.
	void VerifyAuthorsListOutput()
	{
		ImmutableList<AutoId> authors = FindFairUser(a.User.Name).Authors;
		NetExec(a.Os, $"user la name={a.User.Name}");
		for(int i = 0; i < authors.Count; ++i)
			Assert(authors[i].ToString() == Flow.Log.Messages.ToArray()[Flow.Log.Messages.Count - authors.Count + i].Text[0]);
	}

	void ParamsTest()
	{
		Action<string, ProbeType> a = (s, pt) => { ExpectException(() => NetExec(Oses[0], s)); };

		string fmtcmd = $"account la {{0}}";
		Prober.Probe(fmtcmd, a);
	}
}
