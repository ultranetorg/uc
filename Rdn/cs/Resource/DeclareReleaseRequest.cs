﻿namespace Uccs.Rdn;

public class DeclareReleaseRequest : RdnPpc<DeclareReleaseResponse>//, IBinarySerializable
{
	public ResourceDeclaration[]	Resources { get; set; }

	public override PeerResponse Execute()
	{
		lock(Node.Mcv.Lock)
			RequireMember();

		lock(Node.SeedHub.Lock)
			return new DeclareReleaseResponse {Results = Node.SeedHub.ProcessIncoming(Peer.IP, Resources).ToArray()};
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
