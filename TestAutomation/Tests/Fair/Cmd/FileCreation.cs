using System;
using System.IO;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

[TestType(TestType.CommandLine)]
class FileCreation : FairTest
{
	Actor a;
	AutoId authorId;

	string imagePath;

	public FileCreation() : base("File Creation Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		imagePath = GetTestImagePath();

		a = CreateActor(topUp: true);

		authorId = CreateAuthor(a, "Author", years: 1);

		TopUpAuthor(authorId, st: 2 * GetFileSpacetimeRequirement(imagePath));
		TopUpAuthor(authorId);
	}

	public override void Logic()
	{
		// Image with no MIME.
		CreateAuthorFile(a, authorId, imagePath);

		// Image with wrong MIME (not JPG), exception expected.
		ExpectException(() => CreateAuthorFile(a, authorId, imagePath, FairMime.ImagePng));

		// Image with correct MIME (JPG).
		CreateAuthorFile(a, authorId, imagePath, FairMime.ImageJpg);

		// Text file, but with an image MIME(PNG/JPG).
		string filePath = CreateTestFile("text_file.txt");
		ExpectException(() => CreateAuthorFile(a, authorId, filePath, FairMime.ImageJpg));

		// Text fileimage, no MIME.
		CreateAuthorFile(a, authorId, filePath);

		// TODO: Same but for a site (site can create it's own files too).

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); }

		Prober.Probe($"file c {{0}} path={imagePath} by={a.User.Name}", f);
		Prober.Probe($"file c {FairTable.Author}/{authorId} path={{0}} by={a.User.Name}", f);
		Prober.Probe($"file c {FairTable.Author}/{authorId} path={imagePath} by={{0}}", f);
	}
}
