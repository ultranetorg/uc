using System;

using TestAutomation.ParamProber;
using Uccs.Net;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// Author entity command test.
/// - Base case - checking entity of an existing author.
/// - Non-existing id.
/// </summary>
[TestType(TestType.CommandLine)]
class AuthorEntity : FairTest
{
	User u;
	AutoId authorId;

	public AuthorEntity() : base("Author Entity Query Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		u = CreateUserAccount(Oses[1], topUp: true);
		authorId = CreateAuthor(Oses[1], "Author", 9, u.Name);
		
	}

	public override void Logic()
	{
		NetExec(Oses[1], $"author e id={authorId}");

		// Non existing account id.
		ExpectException(() => NetExec(Oses[1], $"author e id=12345678-0"));

		ParamsTest();
	}

	void ParamsTest()
	{
		Action<string, ProbeType> a = (s, pt) => { ExpectException(() => NetExec(Oses[1], s)); };

		string fmtcmd = $"author e id={{0}}";
		Prober.Probe(fmtcmd, a);
	}
}
