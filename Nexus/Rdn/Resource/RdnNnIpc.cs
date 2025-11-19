namespace Uccs.Rdn;

public abstract class RdnNnIpc<R> : Ipc<R> where R : IppResponse
{
	public RdnNode Node => Owner as RdnNode;
}

public class RdnHolderClassesNnIpc : RdnNnIpc<HolderClassesNnIpr>
{
	public override IppResponse Execute(Flow flow)
	{
		return new HolderClassesNnIpr {Classes = [nameof(Account)]};
	}
}
