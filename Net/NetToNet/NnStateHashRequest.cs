namespace Uccs.Net;

public class NnStateHashRequest : NnPpc<NnStateHashResponse>
{
	public string			Net { get; set; }

	public NnStateHashRequest()
	{
	}
	
	public override PeerResponse Execute()
	{
		return new NnStateHashResponse {Hash = Peering.GetStateHash(Net)};
	}
}

public class NnStateHashResponse : PeerResponse
{
	public byte[]	Hash { get; set; }
}
