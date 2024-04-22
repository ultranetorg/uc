namespace Uccs.Net
{
	public class DeclareReleaseRequest : RdcCall<DeclareReleaseResponse>//, IBinarySerializable
	{
		public ResourceDeclaration[]	Resources { get; set; }
		public override bool			WaitResponse => true;

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
				RequireMember(sun);

			lock(sun.SeedHub.Lock)
				return new DeclareReleaseResponse {Results = sun.SeedHub.ProcessIncoming(Peer.IP, Resources).ToArray() };
		}
	}

	public enum DeclarationResult
	{
		None, Accepted, ResourceNotFound, Rejected, NotRelease, NotNearest, 
	}

	public class ReleaseDeclarationResult
	{
		public ReleaseAddress		Address { get; set; }
		public DeclarationResult	Result { get; set; }	

		public ReleaseDeclarationResult()
		{
		}

		public ReleaseDeclarationResult(ReleaseAddress address, DeclarationResult result)
		{
			Address = address;
			Result = result;
		}
	}

	public class DeclareReleaseResponse : RdcResponse
	{
		public ReleaseDeclarationResult[]	Results { get; set; }
	}

}
