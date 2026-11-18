using System;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests `domain transfer` command - transfer of a domain from one user to another.
/// Basic smoke testing; Attempt to transfer non-existent domain; Command parameters input test.
/// </summary>
[TestType(TestType.CommandLine)]
class DomainTransferCmd_Test : RdnTest
{
	User u1, u2;
	AccountAddress? aa1;
	EntityId? receiverId;
	string domain = "_domain";

	public DomainTransferCmd_Test() : base("Domain Transfer Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		//(aa1, _) = CreateWalletAccount(Oses[1], true);
		//(_, receiverId) = CreateWalletAccount(Oses[2], true);
		//DomainAcquire(Oses[1], domain, aa1.ToString(), 1, TransactionStatus.Confirmed);
		u1 = CreateUserAccount(Oses[1], topUp: true);
		u2 = CreateUserAccount(Oses[2], topUp: true);
		DomainAcquire(Oses[1], domain, u1.Name, 1);
	}

	public void Logic()
	{
		NetExec(Oses[1], $"domain transfer {domain} to={receiverId} signer={aa1} aor=retryuntilconfirmed");
		AssertStatusConfirmedOutput();

		// Transfer of a non-existent domain.
		ExpectException(() => NetExec(Oses[1], $"domain transfer _tuntuntunsahur to={receiverId} signer={aa1} aor=retryuntilconfirmed"));

		ParamsTest();
	}

	void ParamsTest()
	{
		Action<string, ProbeType> a = (s, pt) => ExpectException(() => NetExec(Oses[1], s));

		string fmtcmd = $"domain transfer {{0}} to={receiverId} signer={aa1} aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, a);
		fmtcmd = $"domain transfer {domain} to={{0}} signer={aa1} aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, a);
		fmtcmd = $"domain transfer {domain} to={receiverId} signer={{0}} aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, a);
		fmtcmd = $"domain transfer {domain} to={receiverId} signer={aa1} aor={{0}}";
		Prober.Probe(fmtcmd, a);
	}
}
