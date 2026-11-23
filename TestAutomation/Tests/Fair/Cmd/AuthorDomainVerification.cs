using System;

using TestAutomation.ParamProber;
using Uccs.Net;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// </summary>
[TestType(TestType.CommandLine)]
class AuthorDomainVerification : FairTest
{
	Actor a;
	User u;
	AutoId authorId;

	public AuthorDomainVerification() : base("Author Entity Query Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		authorId = CreateAuthor(a, "Author", 1);
	}

	public override void Logic()
	{
		NetExec(a.Os, $"author verification id={authorId} domain=aboba by={a.User.Name}");

	}

}
