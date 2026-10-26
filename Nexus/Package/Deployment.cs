using Uccs.Rdn;

namespace Uccs.Nexus;

public class DeploymentMerge
{
	public Package											Target;
	public Package											Complete;	
	public List<KeyValuePair<Package, ParentPackage>>		Deltas = [];	
}

public class Deployment
{
	public List<DeploymentMerge>	Merges = [];	
}

public class DeploymentMergeProgress
{
	public Ura		Target { get; set; }	
	public Ura		Complete { get; set; }	
	public Ura[]	Deltas { get; set; }	

	public DeploymentMergeProgress()
	{
	}

	public DeploymentMergeProgress(DeploymentMerge merge)
	{
		Target		= merge.Target.Address;
		Complete	= merge.Complete?.Address;
		Deltas		= merge.Deltas.Select(i => i.Key.Address).ToArray();
	}
}

public class DeploymentProgress : PackageActivityProgress
{
	public DeploymentMergeProgress[]	Merges { get; set; }

	public DeploymentProgress()
	{
	}

	public DeploymentProgress(Deployment deployment)
	{
		Merges = deployment.Merges.Select(i => new DeploymentMergeProgress(i)).ToArray();
	}

	public override string ToString()
	{
		return $"deploment: {Merges.Length}, packages: {Merges.Sum(i => i.Deltas.Length + 1)}";
	}
}
