using System.Numerics;

namespace Uccs.Fair;

public class UserRegistration : VotableOperation
{
	public byte[]				Pow { get; set; }
	public override string		Explanation => $"Site={Site}";
	
	public UserRegistration()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Pow = reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteBytes(Pow);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return other is UserRegistration o && o.Signer.Address == Signer.Address;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(Site.Users.Contains(Signer.Id))
		{
			error = AlreadyExists;
			return false;
		}

		if(!execution.SkipPowCheck)
		{
			if(Pow != null && Cryptography.Hash([..execution.Mcv.GraphHash, ..Pow]).Sum(i => BitOperations.PopCount(i)) < Site.PoWComplexity)
			{
				error = DoesNotSatisfy;
				return false;
			}
		}

		error = null;
		return true;
 	}

	public override void PreTransact(McvNode node, bool sponsored, Flow flow, AutoId site)
	{
		if(sponsored)
		{
			Pow = null;

			var r = new Random();
	
			var s = node.Peering.Call(new PowPpc {Site = site}, flow);
	
			var ts =  Enumerable.Range(0, Environment.ProcessorCount)
								.Select(i => new Thread(() =>	{ 
																	var b = new byte[32];
																	var a = Blake2Fast.Blake2b.CreateHashAlgorithm(32);
	
																	while(flow.Active && Pow == null)
																	{
																		r.NextBytes(b);
																		var h = a.ComputeHash([..s.GraphHash, ..b]);
	
																		var f = h.Sum(i => BitOperations.PopCount(i));
															
																		if(f >= s.Complexity)
																		{
																			Pow = b;
																		}
																	}
																})).ToArray();
			foreach(var i in ts)
				i.Start();
					
			foreach(var i in ts)
				i.Join();
		}
	}

	public override void Execute(FairExecution execution)
	{
		var s = Site;

		s.Users = [..s.Users, Signer.Id];

		Signer.Registrations = [..Signer.Registrations, Site.Id];

		if(Pow != null)
		{	
			Signer.AllocationSponsor = new EntityAddress(FairTable.Site, s.Id);
			execution.AllocateForever(s, execution.Net.EntityLength);
		}
		else
			execution.AllocateForever(Signer, execution.Net.EntityLength);
	}
}
