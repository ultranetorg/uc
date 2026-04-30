using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Fair;

public class FairNnpLcpConnection : McvNnpLcpConnection
{
	public new FairNode		Node => base.Node as FairNode;

	public FairNnpLcpConnection(FairNode node, Flow flow) : base(node, flow)
	{
		Classes = [nameof(User), nameof(Author), nameof(Site)];
	}

	protected override void GetHolder(byte table, string n, out ISpacetimeHolder sh, out IEnergyHolder eh)
	{
		if(table == (byte)FairTable.Author)
		{
			var a = Node.Mcv.Authors.Latest(AutoId.Parse(n));
	
			if(a == null)
				throw new EntityException(EntityError.NotFound);
	
			sh = a;
			eh = a;
		}
		else if(table == (byte)FairTable.Author)
		{
			var a = Node.Mcv.Sites.Latest(AutoId.Parse(n));
	
			if(a == null)
				throw new EntityException(EntityError.NotFound);
	
			sh = a;
			eh = a;
		}
		else
			base.GetHolder(table, n, out sh, out eh);
	}
}
