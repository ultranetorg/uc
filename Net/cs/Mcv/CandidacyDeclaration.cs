﻿using System.Net;

namespace Uccs.Net;

public class CandidacyDeclaration : Operation
{
	public IPAddress[]		GraphIPs  { get; set; }

	public override string	Explanation => $"Id={Signer.Id}, Address={Signer.Address}, BaseRdcIPs={string.Join(',', GraphIPs as object[])}";
			
	protected Generator		Affected;

	public CandidacyDeclaration()
	{
	}

	public override bool IsValid(McvNet net) => true;

	public override void Read(BinaryReader reader)
	{
		GraphIPs = reader.ReadArray(() => reader.ReadIPAddress());
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(GraphIPs, i => writer.Write(i));
	}

	public override void Execute(Execution execution)
	{
		if(execution.Round.Members.Any(i => i.Id == Signer.Id))
		{
			Error = "Already member";
			return;
		}

		var c = execution.Candidates.Find(i => i.Id == Signer.Id);

		if(c != null)
		{
			Error = "Already a candidate";
			return;
		}

		Signer.Energy -= execution.Net.DeclarationCost;

		Affected = execution.AffectCandidate(Signer.Id);
		
		Affected.Id			= Signer.Id;
		Affected.Address	= Signer.Address;
		Affected.BaseRdcIPs	= GraphIPs;
		Affected.Registered	= execution.Round.Id;
	}
}
