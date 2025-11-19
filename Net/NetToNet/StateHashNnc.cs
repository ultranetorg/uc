namespace Uccs.Net;

public class StateHashNnc : Nnc<StateHashNnr>
{
	public string			Net { get; set; }

	public StateHashNnc()
	{
	}
	
	public override PeerResponse Execute()
	{
		///return new StateHashNnr {Hash = Peering.GetStateHash(Net)};
		return null;
	}
}

public class StateHashNnr : PeerResponse
{
	public byte[]	Hash { get; set; }
}
