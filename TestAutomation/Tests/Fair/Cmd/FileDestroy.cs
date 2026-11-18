using System;
using System.IO;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;
using System.Diagnostics;

namespace TestAutomation.Tests.Fair;

[TestType(TestType.CommandLine)]
class FileDestroy : FairTest
{
	Actor a;
	AutoId authorId;

	string imagePath;

	public FileDestroy() : base("File Destroy Test") { }

	public override void Init()
	{
		imagePath = GetTestImagePath();

		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		authorId = CreateAuthor(a, "Author", years: 1);
;
		TopUpAuthor(authorId, st: GetFileSpacetimeRequirement(imagePath));

		CreateAuthorFile(a, authorId, imagePath);
	}

	public void Logic()
	{
		DestroyFile(a, McvFind<Author>(authorId).Files[0]);
		Assert(McvFind<Author>(authorId).Files.Length == 0);

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); }

		Prober.Probe($"file x {{0}} by={a.User.Name}", f);
		Prober.Probe($"file x {new AutoId()} by={{0}}", f);
	}
}
