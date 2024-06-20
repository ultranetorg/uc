using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{

	public abstract class InterzoneCall<R> : PeerCall<R> where R : PeerResponse
	{
		public new InterzoneNode	Node => base.Node as InterzoneNode;

	}

}
