namespace Uccs.Rdn;

public class DeclareReleasePpc : RdnPpc<DeclareReleasePpr>//, IBinarySerializable
{
	public ResourceDeclaration[]	Resources { get; set; }

	public override Result Execute()
	{
		lock(Node.Mcv.Lock)
			RequireMember();

		lock(Node.SeedHub.Lock)
			return new DeclareReleasePpr {Results = Node.SeedHub.ProcessIncoming(Peer.IP, Resources).ToArray()};
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

public class DeclareReleasePpr : Result
{
	public ReleaseDeclarationResult[]	Results { get; set; }
}
