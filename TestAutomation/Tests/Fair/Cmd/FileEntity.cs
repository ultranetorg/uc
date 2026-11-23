using System;
using System.IO;
using System.Threading;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

[TestType(TestType.CommandLine)]
class FileEntity : FairTest
{
	Actor a;
	AutoId authorId;

	string imagePath;

	public FileEntity() : base("File Entity Test") { }

	public override void Init()
	{
		imagePath = Path.Join(Directory.GetCurrentDirectory(), "TestData", "test_image.jpg");
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		authorId = CreateAuthor(a, "Author", years: 1);

		TopUpAuthor(authorId, st: GetFileSpacetimeRequirement(imagePath));

		CreateAuthorFile(a, authorId, imagePath);
	}

	public override void Logic()
	{
		NetExec(a.Os, $"file e {McvFind<Author>(authorId).Files[0]}");

		// Non-existent file id. Exception expected.
		ExpectException(() => NetExec(a.Os, $"file e 122334-0"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); }

		Prober.Probe($"file e {{0}}", f);
	}
}
