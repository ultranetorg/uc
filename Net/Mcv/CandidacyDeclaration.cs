using System.Net;

namespace Uccs.Net;

public class CandidacyDeclaration : Operation
{
	public Endpoint[]		GraphEndpoints  { get; set; }
	public AutoId			Beneficiary { get; set; }

	public override string	Explanation => $"{nameof(Beneficiary)}={Beneficiary}, {nameof(GraphEndpoints)}={(GraphEndpoints != null ? string.Join<Endpoint>(',', GraphEndpoints) : null)}";

	public CandidacyDeclaration()
	{
	}

	public override bool IsValid(McvNet net) => true;

	public override void Read(Reader reader)
	{
		Beneficiary	= reader.Read<AutoId>();
		GraphEndpoints	= reader.ReadArray<Endpoint>();
	}

	public override void Write(Writer writer)
	{
 		writer.Write(Beneficiary);
		writer.Write(GraphEndpoints);
	}

	public override void Execute(Execution execution)
	{
		if(execution.Round.Members.Any(i => i.Generator == User.Id))
		{
			Error = "Already member";
			return;
		}

		var c = execution.Candidates.Find(i => i.Generator == User.Id);

		if(c != null)
		{
			Error = "Already a candidate";
			return;
		}

		if(!UserExists(execution, Beneficiary, out _, out Error))
			return;

		User.Energy -= execution.Net.DeclarationCost;

		c = execution.AffectCandidate(User.Id);
		
		c.Generator			= User.Id;
		c.Beneficiary		= Beneficiary;
		c.GraphPpiEndpoints	= GraphEndpoints;

		execution.EnergySpenders.Add(User);
	}
}
