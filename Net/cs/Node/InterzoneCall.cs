namespace Uccs.Net
{

	public abstract class InterzoneCall<R> : PeerCall<R> where R : PeerResponse
	{
		public new InterzoneNode	Node => base.Node as InterzoneNode;

	}

}
