namespace Uccs.Net
{

	public abstract class NexusCall<R> : PeerCall<R> where R : PeerResponse
	{
		public new NexusNode	Node => base.Node as NexusNode;

	}

}
