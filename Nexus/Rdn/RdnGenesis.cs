namespace Uccs.Rdn;

public class RdnGenesis : Genesis
{
	public RdnGenesis()
	{
	}

	protected override void Declare(Execution execution, Generator generator)
	{
		(generator as RdnGenerator).SeedhubPpiEndpoints = [execution.Net.Father0EP];
	}
}
