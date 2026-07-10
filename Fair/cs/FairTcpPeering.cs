using System.Reflection;

namespace Uccs.Fair;

public enum FairPpcClass : uint
{
	None = 0, 
	FairUser = McvPpcClass._Last + 1, 
	FairMembers,

	UserAuthors,
	UserStores,

	Author,
	Product,
	Store,
	StoreCategories,
	Category,
	CategoryPublications,
	CategoryCategories,
	Publication,
	Review,
	Pow,
	File
}

public abstract class FairPpc<R> : McvPpc<R> where R : Result
{
	public new FairTcpPeering	Peering => base.Peering as FairTcpPeering;
	public new FairNode			Node => base.Node as FairNode;
	public new FairMcv			Mcv => base.Mcv as FairMcv;
}

public class FairTcpPeering : McvPeering
{
	public FairTcpPeering(FairNode node, PeeringSettings settings, long roles, VaultApiClient vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
	{
		Constructor.Register<PeerRequest>(Assembly.GetExecutingAssembly(), typeof(FairPpcClass), i => i.Remove(i.Length - "Ppc".Length));
		Constructor.Register<Result>	 (Assembly.GetExecutingAssembly(), typeof(FairPpcClass), i => i.Remove(i.Length - "Ppr".Length));

		Constructor.Register(() => new FairUser(Mcv));

		Run();
	}

	public override bool ValidateIncoming(Operation o)
	{
		/// TODO: remake as async
		/// if(o is AuthorVerification m && !Node.IsWebdomainOwner($"{m.Name}.{m.Tld}", ))
		/// 	return false;

		return o is not VotableOperation;
	}

	public override void OnRequestException(Peer peer, NodeException ex)
	{
		base.OnRequestException(peer, ex);
	}
}
