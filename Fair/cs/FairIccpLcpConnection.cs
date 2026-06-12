using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Fair;

public class FairIccpLcpConnection : McvIccpLcpConnection
{
	public new FairNode		Node => base.Node as FairNode;

	public FairIccpLcpConnection(FairNode node, Flow flow) : base(node, node.Name, flow)
	{
		Classes = [nameof(User), nameof(Author), nameof(Site)];
	}

	public virtual Result Info(string from, InfoIcca args)
	{
		return new InfoIccr {Wayins = [new (){Software = "iccp:/ultranetorg/fns", Command = "https://fair.net"}]};
	}

	protected override void GetHolder(byte @class, AutoId entity, out ISpacetimeHolder sh, out IEnergyHolder eh)
	{
		if(@class == (byte)FairTable.Author)
		{
			var a = Node.Mcv.Authors.Latest(entity)
					??
					throw new EntityException(EntityError.NotFound);
	
			sh = a;
			eh = a;
		}
		else if(@class == (byte)FairTable.Author)
		{
			var a = Node.Mcv.Sites.Latest(entity)
					??
					throw new EntityException(EntityError.NotFound);
	
			sh = a;
			eh = a;
		}
		else
			base.GetHolder(@class, entity, out sh, out eh);
	}
}
