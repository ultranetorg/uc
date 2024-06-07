namespace Uccs.Net
{
	public class DeclareReleaseRequest : RdsCall<DeclareReleaseResponse>//, IBinarySerializable
	{
		public ResourceDeclaration[]	Resources { get; set; }
		public override bool			WaitResponse => true;

		public override PeerResponse Execute()
		{
			lock(Sun.Lock)
			{	
				RequireMember();
			}

			lock(Rds.SeedHub.Lock)
				return new DeclareReleaseResponse {Results = Rds.SeedHub.ProcessIncoming(Peer.IP, Resources).ToArray()};
		}
	}

	public enum DeclarationResult
	{
		None, Accepted, ResourceNotFound, Rejected, NotRelease, NotNearest, 
	}

	public class ReleaseDeclarationResult
	{
		public Urr					Address { get; set; }
		public DeclarationResult	Result { get; set; }	

		public ReleaseDeclarationResult()
		{
		}

		public ReleaseDeclarationResult(Urr address, DeclarationResult result)
		{
			Address = address;
			Result = result;
		}
	}

	public class DeclareReleaseResponse : PeerResponse
	{
		public ReleaseDeclarationResult[]	Results { get; set; }
	}
}
