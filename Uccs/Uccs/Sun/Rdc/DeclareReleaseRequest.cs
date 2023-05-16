namespace Uccs.Net
{
	public class DeclareReleaseRequest : RdcRequest
	{
		public PackageAddressPack	Packages { get; set; }
		public override bool		WaitResponse => false;

		public override RdcResponse Execute(Core core)
		{
			if(core.Seedbase == null) throw new RdcNodeException(RdcNodeError.NotHub);

			core.Seedbase.Add(Peer.IP, Packages.Items);

			return null;
		}
	}
}
