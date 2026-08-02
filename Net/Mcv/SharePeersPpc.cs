namespace Uccs.Net;

public class SharePeersPpc : PeerRequest, IBinarySerializable
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

	public void Read(Reader reader)
	{
		Peers = reader.ReadArray<HomoPeer>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Peers);
	}
}
