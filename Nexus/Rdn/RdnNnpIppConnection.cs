using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Rdn;

public class RdnNnpIppConnection : McvNnpIppConnection<RdnNode, RdnTable>
{
	public RdnNnpIppConnection(RdnNode node, Flow flow) : base(node, [nameof(User)], flow)
	{
	}
}
