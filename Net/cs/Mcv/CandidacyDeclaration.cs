﻿using System.Net;

namespace Uccs.Net;

public class CandidacyDeclaration : Operation
{
	public IPAddress[]		BaseRdcIPs  { get; set; }

	public override string	Description => $"Id={Signer.Id}, Address={Signer.Address}, BaseRdcIPs={string.Join(',', BaseRdcIPs as object[])}";
			
	protected Generator		Affected;

	public CandidacyDeclaration()
	{
	}

	public override bool IsValid(Mcv mcv) => true;

	public override void ReadConfirmed(BinaryReader reader)
	{
		BaseRdcIPs = reader.ReadArray(() => reader.ReadIPAddress());
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(BaseRdcIPs, i => writer.Write(i));
	}

	public override void Execute(Mcv mcv, Round round)
	{
		if(round.Members.Any(i => i.Id == Signer.Id))
		{
			Error = "Already member";
			return;
		}

		var c = round.Candidates.Find(i => i.Id == Signer.Id);

		if(c != null)
		{
			Error = "Already a candidate";
			return;
		}

		Signer.Energy -= mcv.Net.DeclarationCost;

		Affected = round.AffectCandidate(Signer.Id);
		
		Affected.Id			= Signer.Id;
		Affected.Address	= Signer.Address;
		Affected.BaseRdcIPs	= BaseRdcIPs;
		Affected.Registered	= round.Id;
	}
}
