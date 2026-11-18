using System;

using TestAutomation.ParamProber;
using Uccs.Net;
using Uccs.Fair;
using System.Diagnostics;

namespace TestAutomation.Tests.Fair;

[TestType(TestType.CommandLine)]
class AuthorName : FairTest
{
	Actor a;
	AutoId authorId;

	public AuthorName() : base("Author Nickname Change Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		authorId = CreateAuthor(a, "Author", years: 1, topUp: true);
	}

	public void Logic()
	{
		string name = "nicky";
		// Nonexistent author id. Failure expected.
		NetExec(a.Os, $"author n id=123 newname={name} by={a.User.Name} aor=expectfailure");
		Assert(McvFind<Author>(authorId).Name == null);

		// Attempt to reference by title. Title != Name, 2 different properties. Exception expected.
		ExpectException(() => NetExec(a.Os, $"author n name=Author newname={name} by={a.User.Name} aor=expectfailure"));
		Assert(McvFind<Author>(authorId).Name == null);

		NetExec(a.Os, $"author n id={authorId} newname={name} by={a.User.Name}");
		Assert(McvFind<Author>(authorId).Name == name);

		name = "nine_eleven_was_done_by_eeerm";
		NetExec(a.Os, $"author n id={authorId} newname={name} by={a.User.Name}");
		Assert(McvFind<Author>(authorId).Name == name);

		name = "nicky";
		NetExec(a.Os, $"author n id={authorId} newname={name} by={a.User.Name}");
		Assert(McvFind<Author>(authorId).Name == name);

		// Referencing author by name.
		string newName = "i_dont_like_bloody_rituals";
		NetExec(a.Os, $"author n name={name} newname={newName} by={a.User.Name}");
		Assert(McvFind<Author>(authorId).Name == newName);

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); }

		Prober.Probe($"author n id={{0}} newname=nicky by={a.User.Name}", f, 
			exclude: [ProbeType.Numbers, ProbeType.NumbersSpaced, ProbeType.NumbersBig]);
		Prober.Probe($"author n id={authorId} newname={{0}} by={a.User.Name}", f,
			exclude: [ProbeType.Numbers, ProbeType.NumbersSpaced, ProbeType.ShortAccountAddress]);
		Prober.Probe($"author n id={authorId} newname=nicky by={{0}}", f);
	}
}
