namespace Uccs.Rdn;

public class DeploymentMerge
{
	public LocalPackage											Target;
	public LocalPackage											Complete;	
	public List<KeyValuePair<LocalPackage, ParentPackage>>		Incrementals = new();	
}

public class Deployment
{
	public List<DeploymentMerge>	Merges = new ();	
}

public class DeploymentMergeProgress
{
	public Ura		Target { get; set; }	
	public Ura		Complete { get; set; }	
	public Ura[]	Incrementals { get; set; }	

	public DeploymentMergeProgress()
	{
	}

	public DeploymentMergeProgress(DeploymentMerge merge)
	{
		Target			= merge.Target.Resource.Address;
		Complete		= merge.Complete.Resource.Address;
		Incrementals	= merge.Incrementals.Select(i => i.Key.Resource.Address).ToArray();
	}
}

public class DeploymentProgress : ResourceActivityProgress
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
