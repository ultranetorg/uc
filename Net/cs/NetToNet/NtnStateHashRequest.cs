namespace Uccs.Net
{
	public class NtnStateHashRequest : NtnPpc<NtnStateHashResponse>
	{
		public string			Net { get; set; }
		public override bool	WaitResponse => true;

		public NtnStateHashRequest()
		{
		}
		
		public override PeerResponse Execute()
		{
			return new NtnStateHashResponse {Hash = Peering.GetStateHash(Net)};
		}
	}

	public class NtnStateHashResponse : PeerResponse
	{
		public byte[]	Hash { get; set; }
	}
}
