using Uccs.Rdn;

namespace Uccs.Nexus;

public class DeploymentMerge
{
	public Package											Target;
	public Package											Complete;	
	public List<KeyValuePair<Package, ParentPackage>>		Incrementals = new();	
}

public class Deployment
{
	public List<DeploymentMerge>	Merges = new ();	
}

public class DeploymentMergeProgress
{
	public AutoId	Target { get; set; }	
	public AutoId	Complete { get; set; }	
	public AutoId[]	Incrementals { get; set; }	

	public DeploymentMergeProgress()
	{
	}

	public DeploymentMergeProgress(DeploymentMerge merge)
	{
		Target			= merge.Target.Id;
		Complete		= merge.Complete.Id;
		Incrementals	= merge.Incrementals.Select(i => i.Key.Id).ToArray();
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
		return $"deploment: {Merges.Length}, packages: {Merges.Sum(i => i.Incrementals.Length + 1)}";
	}
}
