namespace Uccs.Rdn;

public class DeclareReleasePpc : RdnPpc<DeclareReleasePpr>, IBinarySerializable
{
	public ResourceDeclaration[]	Resources { get; set; }

	public override Result Execute()
	{
		lock(Node.Mcv.Lock)
			RequireMember();

		lock(Node.SeedHub.Lock)
			return new DeclareReleasePpr {Results = Node.SeedHub.ProcessIncoming(Peer.EP, Resources).ToArray()};
	}

	public void Read(Reader reader)
	{
		Resources = reader.ReadArray<ResourceDeclaration>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Resources);
	}
}

public enum DeclarationResult : byte
{
	None, Accepted, ResourceNotFound, Rejected, NotRelease, NotNearest, 
}

public class ReleaseDeclarationResult : IBinarySerializable
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

	public void Write(Writer writer)
	{
		writer.WriteVirtual(Address);
		writer.Write(Result);
	}

	public void Read(Reader reader)
	{
		Address = reader.ReadVirtual<Urr>();
		Result = reader.Read<DeclarationResult>();
	}
}

public class DeclareReleasePpr : Result, IBinarySerializable
{
	public ReleaseDeclarationResult[]	Results { get; set; }

	public void Read(Reader reader)
	{
		Results = reader.ReadArray<ReleaseDeclarationResult>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Results);
	}
}
