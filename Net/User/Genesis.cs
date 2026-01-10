namespace Uccs.Net;

public class Genesis : Operation
{
	public override string		Explanation => "";
	
	public Genesis()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}
	
	protected virtual void Declare(Execution execution, Generator generator)
	{
	}

	public override void Read(BinaryReader reader)
	{
	}

	public override void Write(BinaryWriter writer)
	{
	}

	public override void Execute(Execution execution)
	{
		if(execution.Round.Id != 0)
		{
			Error = NotAvailable;
			return;
		}

		var a = execution.CreateUser(execution.Net.Father0Name, execution.Net.Father0Signer);

		a.Energy		+= execution.Net.EnergyEmission;
		a.Spacetime		+= execution.Net.SpacetimeDayEmission;

		var	c = execution.AffectCandidate(a.Id);

		c.Id			= a.Id;
		c.Address		= a.Owner;
		c.GraphPpcIPs	= [execution.Net.Father0IP];
		c.Registered	= execution.Round.Id;

		Declare(execution, c);
	}
}
