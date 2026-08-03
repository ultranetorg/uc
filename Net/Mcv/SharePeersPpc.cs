namespace Uccs.Net;

public class SharePeersPpc : PeerRequest
{
	public HomoPeer[]			Peers { get; set; }

	public override Result Execute()
	{
		if(Peers.Length > 1000)
			throw new RequestException(RequestError.IncorrectRequest);

		lock(Peering.Lock)
		{
			Peering.RefreshPeers(Peers, Peer);
		}

		return null;
	}

	public override void Read(Reader reader)
	{
		Peers = reader.ReadArray<HomoPeer>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Peers);
	}
}
