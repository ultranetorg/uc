using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

using Uccs.Rdn;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests attempt to download an ivalid resource, one that was created with a bad Rrrh.
/// </summary>
[TestType(TestType.Generic)]
class InvalidResourceDownload : RdnTest
{
	Actor a1, a2;
	string domain;

	public InvalidResourceDownload() : base("Wrong Data Type Resource Creation Testset") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor();
		a2 = CreateActor();
		domain = GetUnusedLongDomainName();
		AcquireDomainWithDomainName(a1, domain, years: 1);
	}

	public override void Logic()
	{
		// Creation of the resource with incorrect URN (Unique Resource Name).
		Urn badUrn = GetZeroUrn();
		Ura ura = new(domain, "file");
		CreateResource(a1, ura.ToString(), $"{Meaning.ExternalContent}/{ContentType.Directory}{{address={badUrn}}}");

		// Attempt to download resource that was previously created with incorrect Rrrh.
		// Empty retries should lead to no effect - no file should be downloaded/started.
		string potential_file = "no_such_file.txt";
		NetExec(a2.Os, $"resource download address={ura} localpath={potential_file} wait=no");
		Thread.Sleep(3 * 1000); // 3 second.
		NetExec(a2.Os, $"resource dx address={badUrn}");

		Assert(!File.Exists(potential_file), "This file should not exist. Nothing should have been downloaded.");
	}
}
