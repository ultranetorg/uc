using System;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests `economy cost` command - network fee information query. Command parameters input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class EconomyCost : RdnTest
{
	public EconomyCost() : base("Economy Cost Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public void Logic()
	{
		NetExec(Oses[1], $"economy cost aor=retryuntilconfirmed");
		ValidateOutput();

		// Using rate
		NetExec(Oses[1], $"economy cost rate=1");
		ValidateOutput();


		// Negative rate
		NetExec(Oses[1], $"economy cost rate=-3");
		ValidateOutput();
		ParamsTest();
	}

	void ValidateOutput()
	{
		if(LogContains("Fee") == null) throw new Exception("Ouput contains no \"Fee\" sub-string");
		if(LogContains("Domains Rent") == null) throw new Exception("Output contains no \"Domains Rent\" sub-string");
		if(LogContains("Resource Rent") == null) throw new Exception("Output contains no \"Resource Rent\" sub-string");
		if(LogContains("Resource Data Per Byte Rent") == null) throw new Exception("Output contains no \"Resource Data Per Byte Rent\" sub-string");
	}

	void ParamsTest()
	{
		Action<string, ProbeType> a = (s, pt) => ExpectException(() => NetExec(Oses[1], s));

		string fmtcmd = $"economy cost rate={{0}}";
		Prober.Probe(fmtcmd, a, exclude: [ProbeType.Numbers, ProbeType.NumberDecimal, ProbeType.NumbersSpaced, ProbeType.NumbersBig]);
	}
}
