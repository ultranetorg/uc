using System.Net;

namespace Uccs.Net;

public class CandidacyDeclaration : Operation
{
	public Endpoint[]		GraphEPs  { get; set; }

	public override string	Explanation => $"User={User?.Id}, Owner={User?.Owner}, GraphEPs={string.Join(',', (GraphEPs ?? []) as object[])}";

	public CandidacyDeclaration()
	{
	}

	public override bool IsValid(McvNet net) => true;

	public override void Read(BinaryReader reader)
	{
		GraphEPs = reader.ReadArray<Endpoint>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(GraphEPs);
	}

	public override void Execute(Execution execution)
	{
		if(execution.Round.Members.Any(i => i.User == User.Id))
		{
			Error = "Already member";
			return;
		}

		var c = execution.Candidates.Find(i => i.User == User.Id);

		if(c != null)
		{
			Error = "Already a candidate";
			return;
		}

		User.Energy -= execution.Net.DeclarationCost;

		c = execution.AffectCandidate(User.Id);
		
		c.User			= User.Id;
		c.GraphPpcIPs	= GraphEPs;

		execution.EnergySpenders.Add(User);
	}
}
