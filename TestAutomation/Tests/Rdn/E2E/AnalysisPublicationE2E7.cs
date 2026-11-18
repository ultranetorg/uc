using System.IO;
using Uccs;
using Uccs.Rdn;
using Uccs.Net;

namespace TestAutomation.Tests.Rdn;

using Rdn = Uccs.Rdn.Rdn;

/// <summary>
/// This e2e test represents a use case when user A creates a file resource, and creates 'analysis request'.
/// Resource is then being analyzed by the earlier created consil members, analysis results are uploaded.
/// Consil/analysis resource creation and analysis update transaction integrity is checked.
/// </summary>
[TestType(TestType.EndToEnd)]
class AnalysisPublication_E2E7 : RdnTest
{
	Actor analizer, creator;

	readonly string domain1 = GetUnusedLongDomain();
	readonly string domain2 = GetUnusedLongDomain();
	readonly string counsilRes, analysisRes;
	uint domain1Years = 3;
	uint domain2Years = 7;

	uint AccAddrByteSize => 37; // 0(empty tag) + 32(key) + 1(tag len) + 2(format) + 2(algo);


	public AnalysisPublication_E2E7() : base("Analysis Publication E2E Test")
	{
		counsilRes = $"/{domain1}/consil";
		analysisRes = $"/{domain2}/request";
	}

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public void Logic()

	{
		// TODO: Test idea - what happens if analysis result is being published by someone who is not an "analyzer" member of the consil?

		// Creator of the analysis consil, one of the analyzers in this consil.
		analizer = CreateActor(topUp: true);

		// This is user A - create of the resource and requestor of the analysis.
		creator = CreateActor(topUp: true);
		
		// Domain where consil lives.
		AcquireDomain(analizer, domain1, domain1Years);
		// Domain for resource to analise and analysis resource.
		AcquireDomain(creator, domain2, domain2Years);

		// Consil resource creation ⬇️⬇️⬇️
		(long stBefore_bd, long eBefore) = GetBalance(analizer.User.Name);
		uint sefm = 100;          // Size-Energy Fee Minimum (Bonus reward for the size of the resource that is analized)
		uint refm = 5;          // Result Energy Fee Minimum
		uint rstfm = 10;         // Result Space-Time Fee Minimum
		AccountAddress[] zers = [Net.Father0Signer, analizer.User.Owner];
		Uccs.Rdn.ResourceCreation counsilRC = NetExec(analizer.Os, $"resource c {counsilRes} data={nameof(DataType.Data)}{{type={ContentType.Ampp_Council} " +
			$"analyzers={zers[0]},{zers[1]} sefm={sefm} refm={refm} rstfm={rstfm}}} by={analizer.User.Name}") as Uccs.Rdn.ResourceCreation;
		(long stAfter_bd, long eAfter) = GetBalance(analizer.User.Name);
		long stDiff_by = (stAfter_bd - stBefore_bd) / 365;
		long eDiff = eAfter - eBefore;

		/*
			Counsil resource layout
			+-------+------------+-----------+----------+----------+-----+
			| Bytes | ?          | 1         | 20       | 20       | ...
			+-------+------------+-----------+----------+----------+-----+
			| Data  | 7 Bit      | Analyzers | Analyzer | Analyzer | ...
			|       | Encoded    | Count     | Account  | Account  |
			|       | Parameters |           | Address  | Address  |
			+-------+------------+-----------+----------+----------+-----+
		*/
		var fs = new FakeStream();
		var bWriter = new BinaryWriter(fs);
		bWriter.Write7BitEncodedInt64(sefm);
		bWriter.Write7BitEncodedInt64(refm);
		bWriter.Write7BitEncodedInt64(rstfm);
		long stDiffExpected_by = -(fs.Length + 1 + AccAddrByteSize * zers.Length + 100) * domain1Years;
		//long stDiffExpected_by = -(fs.Length + 1 + 37 * zers.Length + 100) * domain1Years;
		Assert(stDiffExpected_by == stDiff_by, $"Bad transaction integrity. " +
			$"Consil creator is expected to be fined " +
			$"{stDiffExpected_by}byte/years, but {stDiff_by}byte/years were fined.");

		// Resource-to-analyze creation - single file.
		string path = CreateMinimalReleaseFile("releaseFile.txt");
		LocalReleaseApe lra = CreateRelease(creator.Os, path);
		CreateResource(creator, $"/{domain2}/resource", $"{nameof(DataType.File)}{{address={lra!.Address}}}");

		// Creation of analysis "request".
		uint erwrd = (sefm + refm) * (uint)zers.Length;   // sefm + refm * analizers_count
		uint strwrd = rstfm * (uint)zers.Length;          // rstfm * analizers_count
		(stBefore_bd, eBefore) = GetBalance(creator.User.Name);
		NetExec(creator.Os, $"resource c {analysisRes} data={nameof(DataType.Data)}{{type={ContentType.Ampp_Analysis} " +
			$"release={lra.Address} ereward={erwrd} streward={strwrd} consil={counsilRes}}} by={creator.User.Name}");
		(stAfter_bd, eAfter) = GetBalance(creator.User.Name);
		stDiff_by = (stAfter_bd - stBefore_bd) / 365;

		/*
			Analysis resource layout
			+-------+--------------+------------------+----------+--------------+--------------+-----+
			| Bytes | 1 +          | sizeof(EntityId) | 1        | 2            | 2            | ... |
			|       | sizeof(Hash) |                  |          |              |              |     |
			+=======+==============+==================+==========+==============+==============+=====+
			| Data  | Release      | Counsil          | Analysis | Analyzer idx | Analyzer idx |     |
			|       | Urr          | Resource         | Result   | +            | +            |     |
			|       | (Scheme +    | Entity ID        | Count    | Analysis     | Analysis     |     |
			|       | Hash)        |                  |          | Result       | Result       |     |
			+-------+--------------+------------------+----------+--------------+--------------+-----+
		*/
		fs.Position = 0; // Reuse fake stream
		lra.Address.WriteVirtual(bWriter); // Write Release Urr
		bWriter.Write(FindResource(counsilRC.Address).Id); // Write entity ID of the counsil resource
		bWriter.Write7BitEncodedInt64(erwrd);
		bWriter.Write7BitEncodedInt64(strwrd);
		// Rdn analysis resource contains analysis result records. One byte contains count of results written. We dont have any yet - count is == 0,
		// so we need to write a one empty byte.
		bWriter.Write7BitEncodedInt64(0);

		stDiffExpected_by = -(fs.Length + 100) * domain2Years;
		Assert(stDiffExpected_by == stDiff_by, $"Bad transaction integrity. Analysis resource creator is expected to be fined {stDiffExpected_by}byte/years, but {stDiff_by}byte/years were fined.");

		CreateLink(creator, $"/{domain2}/request", $"/{domain2}/resource");

		AnalysisPublishWithCheck(analizer.Os, analizer.User.Name, creator.User.Name, rstfm);
		AnalysisPublishWithCheck(Oses[0], Net.Father0Name, creator.User.Name, rstfm);

		File.Delete(path);

	}

	void AnalysisPublishWithCheck(Os lizerOs, string lizer, string author, uint rstfm)
	{
		// Analizers publishing analysis results.
		(long stBeforeAzer_bd, long eBeforeAzer) = GetBalance(lizer);
		(long stBeforeAuthor_bd, long eBeforeAuthor) = GetBalance(author);
		NetExec(lizerOs, [new AnalysisResultUpdation { Analysis = FindResource(new Ura(domain2, "request")).Id, Result = AnalysisResult.Positive}], lizer);
		(long stAfterAzer_bd, long eAfterAzer) = GetBalance(lizer);
		(long stAfterAuthor_bd, long eAfterAuthor) = GetBalance(author);
		long stDiffAzer_bd = stAfterAzer_bd - stBeforeAzer_bd;
		long eDiffAzer = eAfterAzer - eBeforeAzer;
		long stDiffAuthor_bd = stAfterAuthor_bd - stBeforeAuthor_bd;
		long eDiffAuthor = eAfterAuthor - eBeforeAuthor;

		Assert(stDiffAzer_bd + stDiffAuthor_bd == 0, $"Bad transaction integrity. Analizer should receive {rstfm}byte/days from resource author, " +
			$"but analizer received {stDiffAzer_bd}byte/days and author lost {stDiffAuthor_bd}byte/days.");

		// -1 because author pays energy for the analysis update command itself too
		Assert(eDiffAzer + eDiffAuthor == 0, "Bad transaction integrity.");

	}
}
