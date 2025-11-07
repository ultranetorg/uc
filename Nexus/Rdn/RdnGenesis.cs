namespace Uccs.Rdn;

public class RdnGenesis : Genesis
{
	public override string		Explanation => GetType().Name;
	
	public RdnGenesis()
	{
	}

	protected override void Declare(Execution execution, Generator generator)
	{
		(generator as RdnGenerator).SeedHubPpcIPs = [execution.Net.Father0IP];
	}
}
