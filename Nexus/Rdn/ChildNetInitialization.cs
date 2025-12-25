namespace Uccs.Rdn;

public class ChildNetInitialization : RdnOperation
{
	public AutoId		Domain  { get; set; }
	public NnpState		Net { get; set; }

	public override string		Explanation => $"Hash={Net.Hash.ToHex()}, Pees={Net.Peers.Length}";
	
	public ChildNetInitialization ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if(Net.Hash.Length > 1024)
			return false;
		
		if(Net.Peers.Length > 1000)
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Domain	= reader.Read<AutoId>();
		Net		= reader.Read<NnpState>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Domain);
		writer.Write(Net);
	}

	public override void Execute(RdnExecution execution)
	{
		if(RequireSignerDomain(execution, Domain, out var e) == false)
			return;

		if(e.NnChildNet != null)
		{
			Error = AlreadyExists;
			return;
		}

		if(!Uccs.Rdn.Domain.IsRoot(e.Address))
		{
			Error = NotRoot;
			return;
		}

		e = execution.Domains.Affect(Domain);

 		e.NnChildNet = Net;
	
		execution.PayCycleEnergy(User);
	}
}
