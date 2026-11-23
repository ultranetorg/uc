using System;
using Uccs.Net;

using TestAutomation.ParamProber;
using Uccs.Rdn;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Check that domain create with `domain c` command works as expected.
/// Checks for cases when domain is already owned by other account; 
/// When no domain name provided;
/// Each command parameter is input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class DomainCreation : RdnTest
{
	Actor a1, a2;
	string longAvailableDomainName = GetUnusedLongDomainName();
	string shortAvailableDomainName = "qwertyb";

	public DomainCreation() : base("Domain Acquire Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor(topUp: true);
		a2 = CreateActor(topUp: true);
	}

	public override void Logic()
	{
		AcquireDomainName(a1, longAvailableDomainName, 1);
		CreateDomain(a1, longAvailableDomainName, 1);
		AssertStatusConfirmedOutput();

		// Attepting to acquire already rented domain by another user.
		CreateDomain(a2, longAvailableDomainName, 1, ActionOnResult.ExpectFailure);

		// Attepting to acquire domain with the available short name.
		AcquireDomainName(a2, shortAvailableDomainName, 1);
		CreateDomain(a2, shortAvailableDomainName, 1);

		// Attepting to acquire domain with unavailable known web domain name. Failure expected.
		CreateDomain(a2, "google", 1, ActionOnResult.ExpectFailure);

		// Attempt to acquire domain by nonexistent user. Exception expected.
		ExpectException(() => CreateDomain(a2.Os, GetUnusedLongDomainName(), "bludauserdumass", 1, ActionOnResult.ExpectFailure));

		// TODO krs Replace this with a param prober check.
		// Empty domain name parameter.
		// This works, it grabs "by" as a domain name. This is ugly, but accrding to @Maximion it's ok for now.
		//NetExec(Oses[1], $"domain a by={a1.User.Name} years=1 aor=expectfailure");

		// TODO krs Replace this with a prober check.
		// Huge name (bigger than allowed maximum).
		string hugeStr = GetHugeLoremIpsum(1024 * 2, specialSymbols: false, spaces: false); // 2 mb string

		// @Fix: https://www.notion.so/Inability-to-create-a-domain-with-a-huge-name-1cb4e2aecdb98028a15eeba2ac88b997?pvs=4
		// Huge 2MB domain name. Should throw exception which it does after a fix, nice.
		// This should be moved to ParamProber in future.
		// Note: Allowed domain length range is 1 - 256(inclusive, see DomainName.cs:43).
		ExpectException(() => CreateDomain(a1.Os, $"_{hugeStr}", a1.User.Name, 1));

		// Empty domain name. Exception expected.
		ExpectException(() => CreateDomain(a1.Os, "\"\"", a1.User.Name, 1));

		ParamsTest();
	}

	void ParamsTest()
	{
		Action<string, ProbeType> a = (s, pt) => ExpectException(() => NetExec(a1.Os, s));
		Action<string, ProbeType> f = (s, pt) => { s = s.Replace("retryuntilconfirmed", "expectfailure"); NetExec(a1.Os, s); };

		//ActionOnResult.
		string fmtcmd = $"domain a {{0}} by={a1.User.Name} years=1 aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, a, exclude: [ProbeType.Numbers, ProbeType.NumbersSpaced, 
			ProbeType.NumbersBig, ProbeType.ShortAccountAddress]);
		fmtcmd = $"domain a {longAvailableDomainName} by={{0}} years=1 aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, a);
		fmtcmd = $"domain a {longAvailableDomainName} by={a1.User.Name} years={{0}} aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, a);
		fmtcmd = $"domain a {longAvailableDomainName} by={a1.User.Name} years=1 aor={{0}}";
		Prober.Probe(fmtcmd, a);
	}
}